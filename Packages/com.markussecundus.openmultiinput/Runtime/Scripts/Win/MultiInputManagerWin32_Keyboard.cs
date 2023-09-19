using System;
using System.Collections.Generic;
using UnityEngine;

using KeyboardHandle = System.IntPtr;

namespace MarkusSecundus.MultiInput.Windows
{

#if PLATFORM_STANDALONE_WIN
    internal partial class MultiInputManagerWin32 : MonoBehaviour, IInputProvider
    {
        Dictionary<KeyboardHandle, Keyboard> _activeKeyboards = new();
        Keyboard _getOrCreateKeyboard(KeyboardHandle h)
        {
            if (_activeKeyboards.TryGetValue(h, out var ret)) return ret;

            ret = new Keyboard { Id = (int)h };
            _activeKeyboards[h] = ret;
            OnKeyboardActivated?.Invoke(ret);

            return ret;
        }

        internal class Keyboard : IKeyboard
        {
            public int Id { get; internal set; }


            readonly HashSet<KeyCode> _pressedDown = new(), _pressed = new(), _pressedUp = new();

            public bool IsActive { get; set; }

            public bool IsAnyButtonDown => _pressedDown.Count > 0;

            public bool IsAnyButtonUp => _pressedUp.Count > 0;

            public bool IsAnyButtonPressed => _pressed.Count > 0;

            public IKeyboard.IConfiguration Config { get; }

            public IReadOnlyCollection<KeyCode> ButtonsDown => _pressedDown;

            public IReadOnlyCollection<KeyCode> ButtonsUp => _pressedUp;

            public IReadOnlyCollection<KeyCode> ButtonsPressed => _pressed;


            public bool GetButton(KeyCode buttonNumber) => _pressed.Contains(buttonNumber);

            public bool GetButtonDown(KeyCode buttonNumber) => _pressedDown.Contains(buttonNumber);

            public bool GetButtonUp(KeyCode buttonNumber) => _pressedUp.Contains(buttonNumber);

            public void UpdateState(IEnumerable<Native.KeypressDescriptor> events)
            {
                _pressedDown.Clear();
                foreach (var item in _pressedUp) _pressed.Remove(item);
                _pressedUp.Clear();
                foreach (var e in events)
                {
                    var keycode = Native.NativeVirtualKeyCodeToManagedKeyCode(e.VirtualKeyCode, e.ScanCode);
                    if (keycode == KeyCode.None) continue;

                    if (e.PressState == Native.KeypressDescriptor.State.PRESS_DOWN)
                    {
                        if (_pressed.Add(keycode)) _pressedDown.Add(keycode);
                    }
                    else if (e.PressState == Native.KeypressDescriptor.State.PRESS_UP)
                    {
                        _pressedUp.Add(keycode);
                    }
                    else throw new ArgumentException($"Invalid keypress state {e.PressState}");
                }
            }
        }
    }
#endif
}