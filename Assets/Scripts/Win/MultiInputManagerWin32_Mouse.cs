using System.Collections.Generic;
using UnityEngine;


using DisplayIdentifier = System.Int32;

using MouseHandle = System.IntPtr;
using UnityEngine.UI;
using UnityEngine.Rendering;
using System.Resources;
using Unity.VisualScripting;

#if PLATFORM_STANDALONE_WIN
public partial class MultiInputManagerWin32 : MonoBehaviour
{

    Dictionary<MouseHandle, Mouse> _mice = new();
    Mouse _getOrCreateMouse(MouseHandle h)
    {
        if (_mice.TryGetValue(h, out var ret)) return ret;

        if (Native.GetMouseInfo(h, out var info) == 0) Debug.Log($"Could not get info for mouse {h}", this);
        else Debug.Log($"m{h}:::{info}", this);
        Debug.Log($"Cam({Camera.main.pixelWidth}x{Camera.main.pixelHeight}), SCam({Camera.main.scaledPixelWidth}x{Camera.main.scaledPixelHeight}), Scr({Screen.width}x{Screen.height})", this);


        var color = CursorColors[_mice.Count % CursorColors.Length];
        var cam = Camera.allCameras[_mice.Count % Camera.allCameras.Length];
        var cursor = _createMouseCursor(cam, -1, null, color);


        ret = new Mouse { _inputManager = this, CursorObject = cursor, CursorColor = color };
        ret.Config.TargetCamera = cam;
        
        _mice[h] = ret;
        return ret;

    }
    static readonly Color[] CursorColors = new[] { Color.red, Color.yellow, Color.blue, Color.green, Color.cyan, Color.magenta };


    const string PathToCursorPrefabResource = "OpenMultiInput/MultiMouseCursor";
    Dictionary<DisplayIdentifier, Canvas> _canvasPerDisplay = new Dictionary<DisplayIdentifier, Canvas>();
    Canvas _getCanvasForDisplay(DisplayIdentifier targetDisplay)
    {
        if (_canvasPerDisplay.TryGetValue(targetDisplay, out var canvas))
            return canvas;
        
        canvas = new GameObject($"[CursorCanvas{targetDisplay}]").AddComponent<Canvas>();
        canvas.transform.SetParent(this.transform, false);

        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.targetDisplay = targetDisplay;
        //TODO: set very high sorting layer (must be first created)
        Debug.Log($"display..{targetDisplay}->{canvas.targetDisplay}");

        return _canvasPerDisplay[targetDisplay] = canvas;
    }
    Image _createMouseCursor(Camera cam, int cursorId, Texture texture, Color color)
    {
        var canvas = _getCanvasForDisplay(cam.targetDisplay);
        var cursor = Resources.Load(PathToCursorPrefabResource).GetComponent<Image>();
        cursor = GameObject.Instantiate(cursor);
        cursor.rectTransform.SetParent(canvas.transform, false);
        cursor.rectTransform.localPosition = Vector3.zero;
        cursor.rectTransform.localScale = Vector3.one;
        cursor.rectTransform.localRotation = Quaternion.identity;
        
        return cursor;
    }

    class Mouse : IMouse
    {
        internal MultiInputManagerWin32 _inputManager;

        Vector2 _position;
        public Vector2 ViewportPosition { get => _position.Clamp(Vector2.zero, Config.TargetCamera.PixelWidthHeight()); set => _position = value.Clamp(Vector2.zero, Config.TargetCamera.PixelWidthHeight()); }
        public Vector2 ViewportPositionNormalized => ViewportPosition / Config.TargetCamera.PixelWidthHeight();
        public Vector2 ScreenPosition => Config.TargetCamera.ViewportToScreenPoint(ViewportPositionNormalized);
        public Ray WorldPositionRay => Config.TargetCamera.ScreenPointToRay(ScreenPosition);

        public float ScrollDelta { get; private set; }

        public Vector2 Axes { get; private set; }
        public Vector2 AxesRaw { get; private set; }


        internal Native.MouseButtonPressFlags buttonFlags;

        public bool GetButton(MouseKeyCode buttonNumber) => (int)(buttonFlags & buttonNumber.GetButtonPressedFlag()) != 0;

        public bool GetButtonDown(MouseKeyCode buttonNumber) => (int)(buttonFlags & buttonNumber.GetButtonDownFlag()) != 0;

        public bool GetButtonUp(MouseKeyCode buttonNumber) => (int)(buttonFlags & buttonNumber.GetButtonUpFlag()) != 0;

        public bool IsButtonDown => (int)(buttonFlags & Native.MouseButtonPressFlags.RI_MOUSE_BUTTON_DOWN_BLOCK) != 0;
        public bool IsButtonUp => (int)(buttonFlags & Native.MouseButtonPressFlags.RI_MOUSE_BUTTON_UP_BLOCK) != 0;
        public bool IsButtonPressed => (int)(buttonFlags & Native.MouseButtonPressFlags.RI_MOUSE_BUTTON_PRESSED_BLOCK) != 0;

        public IMouse.Configuration Config { get; set; } = IMouse.Configuration.Default;

        public bool ShouldDrawCursor { get; set; } = true;
        internal Image CursorObject { get; set; }
        public Texture Cursor { get => CursorObject.mainTexture; set => CursorObject.material.mainTexture = value; }
        public Color CursorColor { get => CursorObject.color; set => CursorObject.color = value;}

        internal void UpdateState(Native.MouseInputFrame frame)
        {
            ProcessMovement();
            ProcessScroll();
            ProcessKeys();
            SetCursor();


            void ProcessMovement()
            {
                ViewportPosition += new Vector2(frame.X * Config.MouseSpeed.x, frame.Y * Config.MouseSpeed.y);

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
            void SetCursor() => DrawCursor();
        }

        /// <summary>
        /// Draws the cursor representing the onscreen position of the mouse. Must be called from OnGUI message handler of some <see cref="UnityEngine.Component"/>
        /// </summary>
        internal void DrawCursor()
        {
            if (!ShouldDrawCursor) return;

            if(CursorObject.canvas.targetDisplay == Config.TargetCamera.targetDisplay)
            {
                var newCanvas = _inputManager._getCanvasForDisplay(Config.TargetCamera.targetDisplay);
                CursorObject.transform.SetParent(newCanvas.transform, false);
            }

            {
                var ray = WorldPositionRay;
                Debug.DrawRay(ray.origin - ray.direction * 2, ray.direction * 10, CursorColor);
            }
            
            CursorObject.rectTransform.position = ScreenPosition + new Vector2(CursorObject.rectTransform.sizeDelta.x, -CursorObject.rectTransform.sizeDelta.y)*0.5f;
            return;
        }
    }
}

#endif