using MarkusSecundus.MultiInput;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyboardTest : MonoBehaviour
{
    private void Update()
    {
        foreach(var k in IInputProvider.Instance.ActiveKeyboards)
        {
            if(k.IsAnyButtonDown || k.IsAnyButtonUp)
                Debug.Log($"({Time.frameCount}--{k.Id})-Down: [{string.Concat(k.ButtonsDown)}], Up: [{string.Concat(k.ButtonsUp)}], Press: [{string.Concat(k.ButtonsPressed)}]");
        }
    }
}
