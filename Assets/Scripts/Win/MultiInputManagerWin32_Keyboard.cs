using System;
using System.Collections.Generic;
using UnityEngine;

using KeyboardHandle = System.IntPtr;


#if PLATFORM_STANDALONE_WIN
internal partial class MultiInputManagerWin32 : MonoBehaviour, IInputProvider
{
    Dictionary<KeyboardHandle, Keyboard> _activeKeyboards = new();
    Keyboard _getOrCreateKeyboard(KeyboardHandle h)
    {
        if (_activeKeyboards.TryGetValue(h, out var ret)) return ret;



        return ret;
    }

    internal class Keyboard : IKeyboard
    {
        public bool IsActive { get; set; }

        public bool IsAnyButtonDown => throw new NotImplementedException();

        public bool IsAnyButtonUp => throw new NotImplementedException();

        public bool IsAnyButtonPressed => throw new NotImplementedException();

        public IKeyboard.IConfiguration Config => throw new NotImplementedException();

        public bool GetButton(KeyCode buttonNumber)
        {
            throw new NotImplementedException();
        }

        public bool GetButtonDown(KeyCode buttonNumber)
        {
            throw new NotImplementedException();
        }

        public bool GetButtonUp(KeyCode buttonNumber)
        {
            throw new NotImplementedException();
        }
    }
}
#endif