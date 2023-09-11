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
using EnvHandle = System.IntPtr;
using InputHandle = System.IntPtr;
using UnityEngine.UI;
using System.Net;

#if PLATFORM_STANDALONE_WIN

public enum RIM_DEVICETYPE : int
{
    MOUSE=0, KEYBOARD=1, HID=2
}

public class MultiInputManager : MonoBehaviour
{
    [DllImport("MultiInputWin32.dll")]
    public static extern InputHandle InitInputHandle();
    [DllImport("MultiInputWin32.dll")]
    public static extern int RunInputInfiniteLoop(InputHandle input);
    [DllImport("MultiInputWin32.dll")]
    public static extern int StopInputInfiniteLoop(InputHandle input);



    [DllImport("MultiInputWin32.dll")]
    public static extern EnvHandle InitDebug(NativeAction<string> format, NativeAction<long> integer, NativeAction<IntPtr> pointer, NativeAction<double> floating, NativeAction<string> cstring, NativeWstringAction wstring, NativeAction flush);
    [DllImport("MultiInputWin32.dll")]
    public static extern void DestroyDebug();


    
    [DllImport("MultiInputWin32.dll")]
    public static extern MouseInputFrame ConsumeMouseState(InputHandle tracker, MouseHandle mouseHandle);

    
    [DllImport("MultiInputWin32.dll")]
    public static extern NativeArray<MouseHandle> GetAvailableDevicesOfType(InputHandle tracker, RIM_DEVICETYPE deviceType);
    [DllImport("MultiInputWin32.dll")]
    public static extern NativeArray<MouseHandle> GetActiveDevicesOfType(InputHandle tracker, RIM_DEVICETYPE deviceType);
    [DllImport("MultiInputWin32.dll")]
    public static extern int GetMouseInfo(MouseHandle mouse, out MouseInfo info);


    public Image debugPrototype;
    public Vector2 speedMultiplier;

    Dictionary<MouseHandle, Mouse> debuggersForMice = new();

    class NativeDebugManager : IDisposable
    {
        NativeAction<string> format;
        NativeAction<long> integer;
        NativeAction<IntPtr> pointer;
        NativeAction<double> floating;
        NativeAction<string> cstring;
        NativeWstringAction wstring;
        NativeAction flush, silentFlush;
        string formatString = "";
        List<object> args = new List<object>();
        public NativeDebugManager()
        {
            InitDebug(
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

        public void Dispose() => DestroyDebug();
    }
    [StructLayout(LayoutKind.Sequential)]
    public struct MouseInputFrame
    {
        public int x, y;
        public int mainScroll, horizontalScroll;
        public ButtonPressFlags buttonFlags;
        public bool wasAbsolute;

        public override string ToString() => $"{(wasAbsolute ? "A":"R")}<({x}, {y})-sc({mainScroll}::{horizontalScroll})-fl({(uint)buttonFlags:x})>";
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct MouseInfo
    {
        public int id;
        public int numberOfButtons;
        public int sampleRate;
        public bool hasHorizontalWheel;
        IntPtr name_;
        public override string ToString() => $"<{id})..{numberOfButtons}b..{sampleRate}hz..{hasHorizontalWheel}>'{name_}'";
    }



    volatile IntPtr inputReaderHandle = IntPtr.Zero;
    volatile NativeDebugManager dbg = null;

    public void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        new Thread(() =>
        {
            dbg = new();
            Debug.Log("Starting new thread for win32 coop", this);

            var inputReaderHandle = this.inputReaderHandle = InitInputHandle();
            if (inputReaderHandle == IntPtr.Zero) return;
            Debug.Log($"Created input window({inputReaderHandle})", this);

            var ret = RunInputInfiniteLoop(inputReaderHandle);

            dbg.Dispose(); dbg = null;

            Debug.Log($"Ending win32 coop thread (ret: {ret})", this);
        }).Start();
    }

    private void Update()
    {
        if (inputReaderHandle != IntPtr.Zero)
        {
            var arr = GetActiveDevicesOfType(inputReaderHandle, RIM_DEVICETYPE.MOUSE).Consume();
            foreach (var handle in arr)
            {
                var state = ConsumeMouseState(inputReaderHandle, handle);
                getMouse(handle).UpdateState(state);
            }

        }

        Mouse getMouse(MouseHandle h)
        {
            if (debuggersForMice.TryGetValue(h, out var ret)) return ret;

            if (GetMouseInfo(h, out var info) == 0) Debug.Log($"Could not get info for mouse {h}", this);
            else Debug.Log($"m{h}:::{info}", this);
            Debug.Log($"Cam({Camera.main.pixelWidth}x{Camera.main.pixelHeight}), SCam({Camera.main.scaledPixelWidth}x{Camera.main.scaledPixelHeight}), Scr({Screen.width}x{Screen.height})", this);


            var cursor = Instantiate(debugPrototype);
            cursor.transform.SetParent(debugPrototype.transform.parent);
            cursor.gameObject.SetActive(true);
            cursor.color = CursorColors[debuggersForMice.Count%CursorColors.Length];
            cursor.rectTransform.position = new Vector2(UnityEngine.Random.Range(100, Camera.main.scaledPixelWidth / 2), UnityEngine.Random.Range(100, Camera.main.scaledPixelHeight / 2));

            ret = new Mouse { Cursor = cursor };

            debuggersForMice[h] = ret;
            return ret;
        }
    }

    static Color[] CursorColors = new[] {Color.white, Color.red, Color.yellow, Color.blue, Color.green, Color.cyan, Color.magenta};


    class Mouse : IMouse
    {
        Vector2 position;
        public Vector2 Position { get => position; set => position = value.Clamp(Config.ScreenBoundary); }

        public float ScrollDelta { get; set; }

        public Vector2 Axes { get; set; }
        public Vector2 AxesRaw { get; set; }


        internal ButtonPressFlags buttonFlags;

        public bool GetButton(MouseKeyCode buttonNumber) => (int)(buttonFlags & buttonNumber.GetButtonPressedFlag()) != 0;

        public bool GetButtonDown(MouseKeyCode buttonNumber) => (int)(buttonFlags & buttonNumber.GetButtonDownFlag()) != 0;

        public bool GetButtonUp(MouseKeyCode buttonNumber) => (int)(buttonFlags & buttonNumber.GetButtonUpFlag()) != 0;

        public bool IsButtonDown => (int)(buttonFlags & ButtonPressFlags.RI_MOUSE_BUTTON_DOWN_BLOCK) != 0;
        public bool IsButtonUp => (int)(buttonFlags & ButtonPressFlags.RI_MOUSE_BUTTON_UP_BLOCK) != 0;
        public bool IsButtonPressed => (int)(buttonFlags & ButtonPressFlags.RI_MOUSE_BUTTON_PRESSED_BLOCK) != 0;

        public IMouse.Configuration Config { get; set; } = IMouse.Configuration.Default;

        public Image Cursor { get; set; }

        internal void UpdateState(MouseInputFrame frame)
        {
            Position += new Vector2(frame.x * Config.MouseSpeed.x, frame.y * Config.MouseSpeed.y);
            ScrollDelta = frame.mainScroll * Config.ScrollSpeed;
            
            var axesRaw = new Vector2(frame.x * Config.AxisScale.x, frame.y * Config.AxisScale.y).Clamp(-1f, 1f);
            Axes = Vector2.Lerp(AxesRaw/*from last frame*/, axesRaw, 0.6f);
            AxesRaw = axesRaw;

            foreach (var keycode in MouseKeyCodeHelpers.AllMouseKeyCodes)
                if (GetButtonUp(keycode))
                {
                    buttonFlags &= ~keycode.GetButtonPressedFlag();
                    //Debug.Log($"Key unpressed: {keycode}");
                }
            buttonFlags &= ~(ButtonPressFlags.RI_MOUSE_BUTTON_UP_DOWN_BLOCK);
            buttonFlags |= (frame.buttonFlags & ButtonPressFlags.RI_MOUSE_BUTTON_UP_DOWN_BLOCK);
            foreach (var keycode in MouseKeyCodeHelpers.AllMouseKeyCodes)
                if (GetButtonDown(keycode))
                {
                    buttonFlags |= keycode.GetButtonPressedFlag();
                    //Debug.Log($"Key pressed: {keycode}");
                }

            Cursor.rectTransform.position = Position;
        }
    }


    public void OnDestroy()
    {
        var inputReaderHandle = this.inputReaderHandle;
        Debug.Log($"Stopping the input window({inputReaderHandle})", this);
        var ret = StopInputInfiniteLoop(inputReaderHandle);
        Debug.Log($"Window stopping result: {ret}", this);
        Cursor.lockState = CursorLockMode.None;
    }
}


public enum ButtonPressFlags : int
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

    RI_MOUSE_BUTTON_DOWN_BLOCK = RI_MOUSE_LEFT_BUTTON_DOWN | RI_MOUSE_RIGHT_BUTTON_DOWN| RI_MOUSE_MIDDLE_BUTTON_DOWN | RI_MOUSE_BUTTON_3_DOWN | RI_MOUSE_BUTTON_4_DOWN,
    RI_MOUSE_BUTTON_UP_BLOCK = RI_MOUSE_LEFT_BUTTON_UP | RI_MOUSE_RIGHT_BUTTON_UP | RI_MOUSE_MIDDLE_BUTTON_UP | RI_MOUSE_BUTTON_3_UP | RI_MOUSE_BUTTON_4_UP,

    RI_MOUSE_LEFT_BUTTON_PRESSED = 1 << 20,
    RI_MOUSE_RIGHT_BUTTON_PRESSED = 1 << 21,
    RI_MOUSE_MIDDLE_BUTTON_PRESSED = 1 << 22,
    RI_MOUSE_BUTTON_3_BUTTON_PRESSED = 1 << 23,
    RI_MOUSE_BUTTON_4_BUTTON_PRESSED = 1 << 24,

    RI_MOUSE_BUTTON_PRESSED_BLOCK = (~0) << 20
}
public static class ButtonFlagsHelpers
{
    public static ButtonPressFlags GetButtonDownFlag(this MouseKeyCode code) => (ButtonPressFlags)(1 << (code.AsMouseKeyIndex() * 2));
    public static ButtonPressFlags GetButtonUpFlag(this MouseKeyCode code) => (ButtonPressFlags)(1 << (code.AsMouseKeyIndex() * 2 + 1));
    public static ButtonPressFlags GetButtonPressedFlag(this MouseKeyCode code) => (ButtonPressFlags)(1 << (code.AsMouseKeyIndex() +20));
}
#endif