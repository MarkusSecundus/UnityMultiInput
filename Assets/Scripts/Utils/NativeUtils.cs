using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace MarkusSecundus.Utils.Native
{
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
}
