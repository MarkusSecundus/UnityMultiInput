using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace MarkusSecundus.Utils.Native
{
    public static class NativeUtils
    {
        public const string MainDllPath = "MultiInputWin32.dll";
    }


    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    public delegate void NativeDestructor(IntPtr toFree);


    [StructLayout(LayoutKind.Sequential)]
    public struct NativeArray<T>
    {

        IntPtr begin;
        IntPtr destructor;
        int length;

        public T[] Consume()
        {
            if (length == 0) return Array.Empty<T>();
            if (length < 0 || begin == IntPtr.Zero || destructor == IntPtr.Zero) throw new ArgumentException("Native array is invalid!");

            var ret = new T[length];

            var elemSize = Marshal.SizeOf<T>();
            IntPtr p = begin;
            for (int t = 0; t < length; ++t)
            {
                ret[t] = Marshal.PtrToStructure<T>(p);
                p += elemSize;
            }

            Marshal.GetDelegateForFunctionPointer<NativeDestructor>(destructor)(begin);
            (begin, length) = (IntPtr.Zero, 0);
            return ret;
        }
    }


    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    public delegate void NativeAction();
    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    public delegate void NativeAction<T>(T arg);

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    public delegate void NativeWstringAction([MarshalAs(UnmanagedType.LPTStr)] string s);



    class NativeDebugEnvironment : IDisposable
    {
        internal static class Native
        {
            public const string DllPath = NativeUtils.MainDllPath;
            [DllImport(DllPath)]
            public static extern void InitDebug(NativeAction<string> format, NativeAction<long> integer, NativeAction<IntPtr> pointer, NativeAction<double> floating, NativeAction<string> cstring, NativeWstringAction wstring, NativeAction flush);
            [DllImport(DllPath)]
            public static extern void DestroyDebug();
        }

        NativeAction<string> format;
        NativeAction<long> integer;
        NativeAction<IntPtr> pointer;
        NativeAction<double> floating;
        NativeAction<string> cstring;
        NativeWstringAction wstring;
        NativeAction flush, silentFlush;
        string formatString = "";
        List<object> args = new List<object>();
        public NativeDebugEnvironment()
        {
            Native.InitDebug(
                 format = s => formatString = s,
                 integer = i => args.Add(i),
                 pointer = p => args.Add(p),
                 floating = d => args.Add(d),
                 cstring = s => args.Add(s),
                 wstring = s => args.Add(s),
                 flush = () =>
                 {
                     try
                     {
                         Debug.Log("native: " + string.Format(formatString, args.ToArray()));
                     }
                     catch
                     {
                         Debug.LogError($"Error during native debug... '{formatString}', args: [{args.MakeString()}]");
                     }
                     formatString = "";
                     args.Clear();
                 }
            );
        }

        public void Dispose() => Native.DestroyDebug();
    }
}
