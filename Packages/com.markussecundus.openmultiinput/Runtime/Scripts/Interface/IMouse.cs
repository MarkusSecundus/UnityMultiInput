using MarkusSecundus.Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarkusSecundus.MultiInput
{
    public interface IMouse
    {
        public bool IsActive { get; set; }
        public Vector2 ViewportPosition { get; }
        public Vector2 ScrollDelta { get; }
        public Vector2 ViewportPositionNormalized { get; }
        public Vector2 ScreenPosition { get; }
        public Ray WorldPositionRay { get; }

        public Vector2 Axes { get; }
        public Vector2 AxesRaw { get; }

        public bool GetButton(MouseKeyCode buttonNumber);
        public bool GetButtonDown(MouseKeyCode buttonNumber);
        public bool GetButtonUp(MouseKeyCode buttonNumber);
        public bool IsAnyButtonDown { get; }
        public bool IsAnyButtonUp { get; }
        public bool IsAnyButtonPressed { get; }

        public IConfiguration Config { get; }
        public bool ShouldDrawCursor { get; set; }
        public Texture Cursor { get; set; }
        public Color CursorColor { get; set; }

        public interface IConfiguration
        {
            public Camera TargetCamera { get; set; }
            public Vector2 MouseSpeed { get; set; }
            public Vector2 AxisScale { get; set; }
            public float ScrollSpeed { get; set; }
        }
    }
}