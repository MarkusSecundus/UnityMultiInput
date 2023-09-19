using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace MarkusSecundus.Utils.Native
{
    internal static class NativeUtils
    {
        const string BasePackagePath =

#if UNITY_EDITOR
            "Packages/com.markussecundus.openmultiinput/"
#else
            ""
#endif
            ;

        public const string MainDllPath =
#if UNITY_EDITOR
            BasePackagePath + "Runtime/dlls/" +
            #endif
            "MultiInputWin32.dll";

        public static List<T> GetList<T>(Action<NativeConsumer<T>> dataSource)
        {
            var ret = new List<T>();
            dataSource(ret.Add);
            return ret;
        }
    }


    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    internal delegate void NativeDestructor(IntPtr toFree);



    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    internal delegate void NativeConsumer<T>(T arg);

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    internal delegate void NativeAction();
    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    internal delegate void NativeAction<T>(T arg);

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    internal delegate void NativeWstringAction([MarshalAs(UnmanagedType.LPTStr)] string s);



    internal class NativeDebugEnvironment : IDisposable
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
                         Debug.Log($"({DateTime.Now.TimeOfDay.TotalMinutes})native: " + string.Format(formatString, args.ToArray()));
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
