using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using Unity.VisualScripting;
using UnityEngine;
using System.Linq;

using MarkusSecundus.Utils.Native;
using TMPro;


using MouseHandle = System.IntPtr;
using InputHandle = System.IntPtr;
using UnityEngine.UI;
using System.Net;

#if PLATFORM_STANDALONE_WIN


internal partial class MultiInputManagerWin32 : MonoBehaviour
{
    internal static class Native
    {
        public const string DllPath = NativeUtils.MainDllPath;
        [DllImport(DllPath)]
        public static extern InputHandle InitInputHandle();
        [DllImport(DllPath)]
        public static extern int RunInputInfiniteLoop(InputHandle input);
        [DllImport(DllPath)]
        public static extern int StopInputInfiniteLoop(InputHandle input);



        [DllImport(DllPath)]
        public static extern MouseInputFrame ConsumeMouseState(InputHandle tracker, MouseHandle mouseHandle);


        [DllImport(DllPath)]
        public static extern NativeArray<MouseHandle> GetAvailableDevicesOfType(InputHandle tracker, RIM_DEVICETYPE deviceType);
        [DllImport(DllPath)]
        public static extern NativeArray<MouseHandle> GetActiveDevicesOfType(InputHandle tracker, RIM_DEVICETYPE deviceType);
        [DllImport(DllPath)]
        public static extern int GetMouseInfo(MouseHandle mouse, out MouseInfo info);


        public enum RIM_DEVICETYPE : int
        {
            MOUSE = 0, KEYBOARD = 1, HID = 2
        }


        public enum MouseButtonPressFlags : int
        {
            RI_MOUSE_LEFT_BUTTON_DOWN = 1 << 0,
            RI_MOUSE_LEFT_BUTTON_UP = 1 << 1,
            RI_MOUSE_RIGHT_BUTTON_DOWN = 1 << 2,
            RI_MOUSE_RIGHT_BUTTON_UP = 1 << 3,
            RI_MOUSE_MIDDLE_BUTTON_DOWN = 1 << 4,
            RI_MOUSE_MIDDLE_BUTTON_UP = 1 << 5,
            RI_MOUSE_BUTTON_3_DOWN = 1 << 6,
            RI_MOUSE_BUTTON_3_UP = 1 << 7,
            RI_MOUSE_BUTTON_4_DOWN = 1 << 8,
            RI_MOUSE_BUTTON_4_UP = 1 << 9,

            RI_MOUSE_BUTTON_UP_DOWN_BLOCK = ~RI_MOUSE_BUTTON_PRESSED_BLOCK,

            RI_MOUSE_BUTTON_DOWN_BLOCK = RI_MOUSE_LEFT_BUTTON_DOWN | RI_MOUSE_RIGHT_BUTTON_DOWN | RI_MOUSE_MIDDLE_BUTTON_DOWN | RI_MOUSE_BUTTON_3_DOWN | RI_MOUSE_BUTTON_4_DOWN,
            RI_MOUSE_BUTTON_UP_BLOCK = RI_MOUSE_LEFT_BUTTON_UP | RI_MOUSE_RIGHT_BUTTON_UP | RI_MOUSE_MIDDLE_BUTTON_UP | RI_MOUSE_BUTTON_3_UP | RI_MOUSE_BUTTON_4_UP,

            RI_MOUSE_LEFT_BUTTON_PRESSED = 1 << 20,
            RI_MOUSE_RIGHT_BUTTON_PRESSED = 1 << 21,
            RI_MOUSE_MIDDLE_BUTTON_PRESSED = 1 << 22,
            RI_MOUSE_BUTTON_3_BUTTON_PRESSED = 1 << 23,
            RI_MOUSE_BUTTON_4_BUTTON_PRESSED = 1 << 24,

            RI_MOUSE_BUTTON_PRESSED_BLOCK = (~0) << 20
        }


        [StructLayout(LayoutKind.Sequential)]
        public struct MouseInputFrame
        {
            public int X, Y;
            public int MainScroll, HorizontalScroll;
            public MouseButtonPressFlags ButtonFlags;
            public bool WasAbsolute;

            public override string ToString() => $"{(WasAbsolute ? "A" : "R")}<({X}, {Y})-sc({MainScroll}::{HorizontalScroll})-fl({(uint)ButtonFlags:x})>";
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MouseInfo
        {
            public int Id;
            public int NumberOfButtons;
            public int SampleRate;
            public bool HasHorizontalWheel;
            IntPtr name_;
            public override string ToString() => $"<{Id})..{NumberOfButtons}b..{SampleRate}hz..{HasHorizontalWheel}>'{name_}'";
        }
    }
}
internal static class MultiInputManagerWin32_Native_MouseButtonPressFlagsHelpers
{
    internal static MultiInputManagerWin32.Native.MouseButtonPressFlags GetButtonDownFlag(this MouseKeyCode code) => (MultiInputManagerWin32.Native.MouseButtonPressFlags)(1 << (code.AsMouseKeyIndex() * 2));
    internal static MultiInputManagerWin32.Native.MouseButtonPressFlags GetButtonUpFlag(this MouseKeyCode code) => (MultiInputManagerWin32.Native.MouseButtonPressFlags)(1 << (code.AsMouseKeyIndex() * 2 + 1));
    internal static MultiInputManagerWin32.Native.MouseButtonPressFlags GetButtonPressedFlag(this MouseKeyCode code) => (MultiInputManagerWin32.Native.MouseButtonPressFlags)(1 << (code.AsMouseKeyIndex() + 20));
}


#endif