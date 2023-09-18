using System;
using System.Collections.Generic;
using UnityEngine;

#if PLATFORM_STANDALONE_WIN
internal partial class MultiInputManagerWin32 : MonoBehaviour, IInputProvider
{
    public IReadOnlyCollection<IMouse> ActiveMice => _activeMice.Values;

    public IReadOnlyCollection<IKeyboard> ActiveKeyboards => throw new NotImplementedException();

    public event Action<IMouse> OnMouseActivated;
    public event Action<IKeyboard> OnKeyboardActivated;

    public static MultiInputManagerWin32 GetInstance()
    {
        if (!_Instance) AutocreateTheInputEnvironment();
        if (!_Instance) throw new InvalidOperationException("Getting instance failed - autocreation was initiated but the instance was still not created successfully!");
        return _Instance;
    }
}
#endif