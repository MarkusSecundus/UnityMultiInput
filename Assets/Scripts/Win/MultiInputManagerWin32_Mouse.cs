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
using static MultiInputManagerWin32;

#if PLATFORM_STANDALONE_WIN
public partial class MultiInputManagerWin32 : MonoBehaviour
{
    [SerializeField] Image debugPrototype;
    [SerializeField] Texture2D cursorTexture;
    [SerializeField] Camera otherCamera;

    Dictionary<MouseHandle, Mouse> _mice = new();
    Mouse _getOrCreateMouse(MouseHandle h)
    {
        if (_mice.TryGetValue(h, out var ret)) return ret;

        if (Native.GetMouseInfo(h, out var info) == 0) Debug.Log($"Could not get info for mouse {h}", this);
        else Debug.Log($"m{h}:::{info}", this);
        Debug.Log($"Cam({Camera.main.pixelWidth}x{Camera.main.pixelHeight}), SCam({Camera.main.scaledPixelWidth}x{Camera.main.scaledPixelHeight}), Scr({Screen.width}x{Screen.height})", this);


        var cursor = Instantiate(debugPrototype);
        cursor.transform.SetParent(debugPrototype.transform.parent);
        cursor.gameObject.SetActive(true);
        cursor.color = CursorColors[_mice.Count % CursorColors.Length];
        cursor.rectTransform.position = new Vector2(UnityEngine.Random.Range(100, Camera.main.scaledPixelWidth / 2), UnityEngine.Random.Range(100, Camera.main.scaledPixelHeight / 2));

        ret = new Mouse { _inputManager = this, Cursor = cursorTexture, CursorImage = cursor };

        _mice[h] = ret;
        return ret;
    }

    class Mouse : IMouse
    {
        internal MultiInputManagerWin32 _inputManager;

        Vector2 _position;
        public Vector2 Position { get => _position; set => _position = value.Clamp(Config.ScreenBoundary); }

        public float ScrollDelta { get; set; }

        public Vector2 Axes { get; set; }
        public Vector2 AxesRaw { get; set; }


        internal Native.MouseButtonPressFlags buttonFlags;

        public bool GetButton(MouseKeyCode buttonNumber) => (int)(buttonFlags & buttonNumber.GetButtonPressedFlag()) != 0;

        public bool GetButtonDown(MouseKeyCode buttonNumber) => (int)(buttonFlags & buttonNumber.GetButtonDownFlag()) != 0;

        public bool GetButtonUp(MouseKeyCode buttonNumber) => (int)(buttonFlags & buttonNumber.GetButtonUpFlag()) != 0;

        public bool IsButtonDown => (int)(buttonFlags & Native.MouseButtonPressFlags.RI_MOUSE_BUTTON_DOWN_BLOCK) != 0;
        public bool IsButtonUp => (int)(buttonFlags & Native.MouseButtonPressFlags.RI_MOUSE_BUTTON_UP_BLOCK) != 0;
        public bool IsButtonPressed => (int)(buttonFlags & Native.MouseButtonPressFlags.RI_MOUSE_BUTTON_PRESSED_BLOCK) != 0;

        public IMouse.Configuration Config { get; set; } = IMouse.Configuration.Default;

        public bool ShouldDrawCursor { get; set; } = true;
        public Image CursorImage { get; set; }
        public Texture2D Cursor { get; set; }

        internal void UpdateState(Native.MouseInputFrame frame)
        {
            ProcessMovement();
            ProcessScroll();
            ProcessKeys();
            SetCursor();


            void ProcessMovement()
            {
                Position += new Vector2(frame.X * Config.MouseSpeed.x, frame.Y * Config.MouseSpeed.y);

                var axesRaw = new Vector2(frame.X * Config.AxisScale.x, frame.Y * Config.AxisScale.y).Clamp(-1f, 1f);
                Axes = Vector2.Lerp(AxesRaw/*from last frame*/, axesRaw, 0.6f);
                AxesRaw = axesRaw;
            }
            void ProcessScroll()
            {
                ScrollDelta = frame.MainScroll * Config.ScrollSpeed;
            }
            void ProcessKeys()
            {
                foreach (var keycode in MouseKeyCodeHelpers.AllMouseKeyCodes)
                    if (GetButtonUp(keycode))
                        buttonFlags &= ~keycode.GetButtonPressedFlag();
                buttonFlags &= ~(Native.MouseButtonPressFlags.RI_MOUSE_BUTTON_UP_DOWN_BLOCK);
                buttonFlags |= (frame.ButtonFlags & Native.MouseButtonPressFlags.RI_MOUSE_BUTTON_UP_DOWN_BLOCK);
                foreach (var keycode in MouseKeyCodeHelpers.AllMouseKeyCodes)
                    if (GetButtonDown(keycode))
                        buttonFlags |= keycode.GetButtonPressedFlag();
            }
            void SetCursor()
            {
                CursorImage.rectTransform.position = Position;
            }
        }
    }
}

#endif