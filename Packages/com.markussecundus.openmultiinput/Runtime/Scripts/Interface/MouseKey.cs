using MarkusSecundus.Utils;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using UnityEngine;

namespace MarkusSecundus.MultiInput
{
    public enum MouseKeyCode
    {
        None = KeyCode.None,
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

        public static IReadOnlyList<MouseKeyCode> AllMouseKeyCodes = CollectionUtils.RangeFromToExclusive((int)MouseKeyCode.LeftButton, (int)MouseKeyCode.Button4 + 1).Cast<MouseKeyCode>().ToArray();

        public static int AsMouseKeyIndex(this MouseKeyCode code) => (int)code - (int)MouseKeyCode.LeftButton;
        public static KeyCode AsKeyCode(this MouseKeyCode code) => (KeyCode)(int)code;
        public static MouseKeyCode AsMouseKeyCode(this KeyCode code) => ((MouseKeyCode)code >= MouseKeyCode.LeftButton && (MouseKeyCode)code <= MouseKeyCode.Button4)? (MouseKeyCode)(int)code : MouseKeyCode.None;
    }
}