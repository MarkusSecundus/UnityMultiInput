using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using Unity.VisualScripting;
using UnityEngine;
using System.Linq;

using MarkusSecundus.Utils.Native;
using TMPro;


using MouseHandle = System.IntPtr;
using InputHandle = System.IntPtr;
using UnityEngine.UI;
using System.Net;
using static MultiInputManagerWin32;

#if PLATFORM_STANDALONE_WIN
public partial class MultiInputManagerWin32 : MonoBehaviour
{
    volatile IntPtr _inputReaderHandle = IntPtr.Zero;

    public void Start()
    {
        try
        {
        }
        catch { }

        Cursor.lockState = CursorLockMode.Locked;
        new Thread(() =>
        {
            using var dbg = new NativeDebugEnvironment();
            Debug.Log("Starting new thread for win32 coop", this);

            var inputReaderHandle = this._inputReaderHandle = Native.InitInputHandle();
            if (inputReaderHandle == IntPtr.Zero) return;
            Debug.Log($"Created input window({inputReaderHandle})", this);

            var ret = Native.RunInputInfiniteLoop(inputReaderHandle);

            Debug.Log($"Ending win32 coop thread (ret: {ret})", this);
        }).Start();
    }

    private void Update()
    {
        if (_inputReaderHandle != IntPtr.Zero)
        {
            var arr = Native.GetActiveDevicesOfType(_inputReaderHandle, Native.RIM_DEVICETYPE.MOUSE).Consume();
            foreach (var handle in arr)
            {
                var state = Native.ConsumeMouseState(_inputReaderHandle, handle);
                _getOrCreateMouse(handle).UpdateState(state);
            }

        }
    }

    static readonly Color[] CursorColors = new[] {Color.white, Color.red, Color.yellow, Color.blue, Color.green, Color.cyan, Color.magenta};


    public void OnDestroy()
    {
        var inputReaderHandle = this._inputReaderHandle;
        Debug.Log($"Stopping the input window({inputReaderHandle})", this);
        var ret = Native.StopInputInfiniteLoop(inputReaderHandle);
        Debug.Log($"Window stopping result: {ret}", this);
        Cursor.lockState = CursorLockMode.None;
    }

}

#endif