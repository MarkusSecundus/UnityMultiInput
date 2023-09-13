using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IMouse
{
    public Vector2 ViewportPosition { get; }
    public float ScrollDelta { get; }

    public Vector2 Axes { get; }
    public Vector2 AxesRaw { get; }

    public bool GetButton(MouseKeyCode buttonNumber);
    public bool GetButtonDown(MouseKeyCode buttonNumber);
    public bool GetButtonUp(MouseKeyCode buttonNumber);
    public bool IsButtonDown { get; }
    public bool IsButtonUp { get; }
    public bool IsButtonPressed { get; }

    public Configuration Config { get; set; }

    [System.Serializable]
    public class Configuration
    {
        public Camera TargetCamera;
        public Vector2 MouseSpeed;
        public Vector2 AxisScale;
        public float ScrollSpeed;


        public static Configuration Default => new Configuration { TargetCamera = Camera.main, MouseSpeed = new Vector2(1, -1), AxisScale = new Vector2(0.12f, -0.12f), ScrollSpeed = 1f/120f };
    }
}
