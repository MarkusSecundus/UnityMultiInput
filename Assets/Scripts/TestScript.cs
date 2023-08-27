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
    public static extern int RunInputLoop(IntPtr env);

    [DllImport("MultiInputWin32.dll")]
    public static extern IntPtr InitEnvironment(Action<string> format, Action<long> integer, Action<double> floating, Action flush);
    [DllImport("MultiInputWin32.dll")]
    public static extern void DestroyEnvironment(IntPtr env);


    static class NativeDebugManager
    {
        static Action<string> format;
        static Action<long> integer;
        static Action<double> floating;
        static Action flush;
        static string formatString = "";
        static List<object> args = new List<object>();
        public static readonly IntPtr Env;
        static NativeDebugManager()
        {
            Env = InitEnvironment(
                 format = s => formatString = s,
                 integer = i => args.Add(i),
                 floating = d => args.Add(d),
                 flush = () => { Debug.Log("native: " + string.Format(formatString, args.ToArray())); formatString = ""; args.Clear(); }
            );
        }
    }
    [StructLayout(LayoutKind.Sequential)]
    public struct MouseInputFrame
    {
        public int x;
        public int y;
    }

    public void Start()
    {
        new Thread(() => 
        {
            Debug.Log("Starting new thread for win32 coop");

            var ret = RunInputLoop(NativeDebugManager.Env);

            Debug.Log($"Ending win32 coop thread (ret: {ret})");
        }).Start();
    }

}
