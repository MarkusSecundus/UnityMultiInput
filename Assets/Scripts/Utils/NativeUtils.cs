using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public static class NativeUtils
{

    [DllImport("MultiInputWin32.dll")]
    public static extern void NativeFree(IntPtr toFree);

    [StructLayout(LayoutKind.Sequential)]
    public struct NativeArray
    {
        IntPtr begin;
        int length;

        public T[] Consume<T>()
        {
            if (length < 0) throw new ArgumentException("Native array is invalid!");

            var ret = new T[length];

            var elemSize = Marshal.SizeOf<T>();
            IntPtr p = begin;
            for(int t = 0; t < length; ++t)
            {
                ret[t] = Marshal.PtrToStructure<T>(p);
                p += elemSize;
            }

            NativeFree(begin);
            (begin, length) = (IntPtr.Zero, 0);
            return ret;
        }
    }

}
