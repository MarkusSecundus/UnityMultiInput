using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity.VisualScripting;
using UnityEngine;

public class TestScript : MonoBehaviour
{
    [DllImport("MultiInputWin32.dll")]
    public static extern int TestFunc(IntPtr env, ref MouseInputFrame frame);
    [DllImport("MultiInputWin32.dll")]
    public static extern int TestFuncRef(IntPtr env, ref MouseInputFrame frame);
    
    [DllImport("MultiInputWin32.dll")]
    public static extern IntPtr TestInitAll(IntPtr env, ulong arg);

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
    static TestScript()
    {
        var env = InitEnv();
        Debug.Log($"env: {env}");
        MouseInputFrame input = default;
        TestFunc(env, ref input);
        TestFunc(env, ref input);
        TestFunc(env, ref input);
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct MouseInputFrame
    {
        public int x;
        public int y;
    }

    public void Start()
    {
    }

    static IntPtr _env = IntPtr.Zero;
    static IntPtr InitEnv()
    {
        return NativeDebugManager.Env;
    }

    public void Update()
    {
        var env = InitEnv();
        MouseInputFrame input = default;
        var ret = TestFunc(env, ref input);
        //Debug.Log($"result: {ret}({input.x}:{input.y})");
    }
}
