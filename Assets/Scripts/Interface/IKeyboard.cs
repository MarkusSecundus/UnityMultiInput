using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IKeyboard
{
    public bool IsActive { get; set; }

    public bool GetButton(KeyCode buttonNumber);
    public bool GetButtonDown(KeyCode buttonNumber);
    public bool GetButtonUp(KeyCode buttonNumber);
    public bool IsAnyButtonDown { get; }
    public bool IsAnyButtonUp { get; }
    public bool IsAnyButtonPressed { get; }

    public IConfiguration Config { get; }

    public interface IConfiguration
    {
        int _placeholder { get; set; }
    }
}
