using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

//[CreateAssetMenu(fileName = nameof(MultiInputConfigWin32), menuName = "MultiInput/ConfigWin32", order = 1)]
public class MultiInputConfigWin32 : ScriptableObject
{
    public bool IsMultiInputEnabled = false;
    [System.Serializable]
    public struct MouseConfig
    {

        public Vector2 MouseMovementSpeed;
        public Vector2 MouseAxisSpeed;
        public Vector2 MouseScrollSpeed;


        /// <summary>
        /// Colors that will be automatically assigned (cyclically in the given order) to new mouse cursors as they activate
        /// </summary>
        [Tooltip("Colors that will be automatically assigned (cyclically in the given order) to new mouse cursors as they activate")]
        public Color[] CursorColors;

        public static MouseConfig Default => new()
        {
            MouseMovementSpeed = new Vector2(1, -1),
            MouseAxisSpeed = new Vector2(0.12f, -0.12f),
            MouseScrollSpeed = Vector2.one * (1f / 120f),

            CursorColors = new Color[] { Color.red, Color.yellow, Color.green, Color.blue }
        };
    }

    [System.Serializable]
    public struct KeyboardConfig
    {
        public int _placeholder;

        public static KeyboardConfig Default => new() { _placeholder = -1 };
    }


    public MouseConfig Mouse = MouseConfig.Default;
    public KeyboardConfig Keyboard = KeyboardConfig.Default;
}
