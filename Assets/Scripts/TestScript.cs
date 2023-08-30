using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using Unity.VisualScripting;
using UnityEngine;

public class TestScript : MonoBehaviour
{
    [DllImport("MultiInputWin32.dll")]
    public static extern int RegisterInputHandle(IntPtr env);
    [DllImport("MultiInputWin32.dll")]
    public static extern IntPtr CreateInputHandle(IntPtr env);
    [DllImport("MultiInputWin32.dll")]
    public static extern int RunInputInfiniteLoop(IntPtr env, IntPtr inputReaderHandle);
    [DllImport("MultiInputWin32.dll")]
    public static extern int StopInputInfiniteLoop(IntPtr env, IntPtr inputReaderHandle);


    public delegate void NativeWstringAction([MarshalAs(UnmanagedType.LPTStr)] string s);

    [DllImport("MultiInputWin32.dll")]
    public static extern IntPtr InitEnvironment(Action<string> format, Action<long> integer, Action<IntPtr> pointer, Action<double> floating, Action<string> cstring, NativeWstringAction wstring, Action flush);
    [DllImport("MultiInputWin32.dll")]
    public static extern void DestroyEnvironment(IntPtr env);


    class NativeDebugManager : IDisposable
    {
        Action<string> format;
        Action<long> integer;
        Action<IntPtr> pointer;
        Action<double> floating;
        Action<string> cstring;
        NativeWstringAction wstring;
        Action flush;
        string formatString = "";
        List<object> args = new List<object>();
        public readonly IntPtr Env;
        public NativeDebugManager()
        {
            Env = InitEnvironment(
                 format = s => formatString = s,
                 integer = i => args.Add(i),
                 pointer = p=> args.Add(p),
                 floating = d => args.Add(d),
                 cstring = s=> args.Add(s),
                 wstring = s=> args.Add(s),
                 flush = () => { Debug.Log("native: " + string.Format(formatString, args.ToArray())); formatString = ""; args.Clear(); }
            );
        }

        public void Dispose() => DestroyEnvironment(Env);
    }
    [StructLayout(LayoutKind.Sequential)]
    public struct MouseInputFrame
    {
        public int x;
        public int y;
    }


    volatile IntPtr inputReaderHandle = IntPtr.Zero;
    volatile NativeDebugManager dbg = null;

    public void Start()
    {
        new Thread(() => 
        {
            dbg = new();
            Debug.Log("Starting new thread for win32 coop");

            if (RegisterInputHandle(dbg.Env) != 1) return;
            var inputReaderHandle = this.inputReaderHandle = CreateInputHandle(dbg.Env);
            Debug.Log($"Created input window({inputReaderHandle})");

            var ret = RunInputInfiniteLoop(dbg.Env, inputReaderHandle);
            Debug.Log($"Ending win32 coop thread (ret: {ret})");
        }).Start();
    }

    public void OnDestroy()
    {
        var inputReaderHandle = this.inputReaderHandle;
        Debug.Log($"Stopping the input window({inputReaderHandle})");
        var ret = StopInputInfiniteLoop(dbg.Env, inputReaderHandle);
        Debug.Log($"Window stopping result: {ret}");
        dbg.Dispose(); dbg = null;
    }
}
