using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FallbackMultiInputProvider : IInputProvider
{
    private FallbackMultiInputProvider() { }
    public static FallbackMultiInputProvider Instance { get; } = new();
    public IReadOnlyCollection<IMouse> ActiveMice { get; } = new Mouse[] { Mouse.Instance };

    public event Action<IMouse> OnMouseActivated;

    private class Mouse : IMouse
    {
        public bool IsActive { get; set; }
        private Mouse() { }
        internal static Mouse Instance = new ();

        public Vector2 ViewportPosition => throw new NotImplementedException();

        public Vector2 ScrollDelta => Input.mouseScrollDelta;

        public Vector2 Axes => new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));

        public Vector2 AxesRaw => new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y"));

        public bool IsAnyButtonDown => MouseKeyCodeHelpers.AllMouseKeyCodes.Any(k => Input.GetKeyDown(k.AsKeyCode()));

        public bool IsAnyButtonUp => MouseKeyCodeHelpers.AllMouseKeyCodes.Any(k => Input.GetKeyUp(k.AsKeyCode()));

        public bool IsAnyButtonPressed => MouseKeyCodeHelpers.AllMouseKeyCodes.Any(k => Input.GetKey(k.AsKeyCode()));

        public IMouse.IConfiguration Config { get; } = new Configuration();
        public bool ShouldDrawCursor { get => UnityEngine.Cursor.visible; set => UnityEngine.Cursor.visible = value; }
        public Texture Cursor { get => throw new NotImplementedException(); set => UnityEngine.Cursor.SetCursor((Texture2D)value,Vector2.zero, CursorMode.Auto); }
        public Color CursorColor { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        private class Configuration : IMouse.IConfiguration
        {
            public Camera TargetCamera { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
            public Vector2 MouseSpeed { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
            public Vector2 AxisScale { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
            public float ScrollSpeed { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        }

        public bool GetButton(MouseKeyCode buttonNumber) => Input.GetKey(buttonNumber.AsKeyCode());

        public bool GetButtonDown(MouseKeyCode buttonNumber) => Input.GetKeyDown(buttonNumber.AsKeyCode());

        public bool GetButtonUp(MouseKeyCode buttonNumber) => Input.GetKeyUp(buttonNumber.AsKeyCode());
    }
}
