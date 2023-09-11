using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum MouseKeyCode
{
    LeftButton = KeyCode.Mouse0,
    RightButton = KeyCode.Mouse1,
    MiddleButton = KeyCode.Mouse2,
    Button3 = KeyCode.Mouse3,
    Button4 = KeyCode.Mouse4,
}

public static class MouseKeyCodeHelpers
{
    public static MouseKeyCode FromIndex(int index) => index < 0 || index > MouseKeyCode.Button4.AsMouseKeyIndex()
                                                        ? throw new System.ArgumentOutOfRangeException($"number {index} is not valid mouse key index!")
                                                        : (index + (MouseKeyCode.LeftButton));

    public static IReadOnlyList<MouseKeyCode> AllMouseKeyCodes = Enumerable.Range((int)MouseKeyCode.LeftButton, (int)MouseKeyCode.Button4).Cast<MouseKeyCode>().ToArray();

    public static int AsMouseKeyIndex(this MouseKeyCode code) => (int)code - (int)MouseKeyCode.LeftButton;
    public static KeyCode AsKeyCode(this MouseKeyCode code) => (KeyCode)(int)code;
}
