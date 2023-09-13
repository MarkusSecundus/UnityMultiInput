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
using System.Runtime.InteropServices.WindowsRuntime;

#if PLATFORM_STANDALONE_WIN
public partial class MultiInputManagerWin32 : MonoBehaviour
{
    static MultiInputManagerWin32 _Instance { get; set; }


    volatile IntPtr _inputReaderHandle = IntPtr.Zero;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void AutocreateTheInputEnvironment()
    {
        var o = new GameObject("[MultiInput_Win32]");
        GameObject.DontDestroyOnLoad(o);
        MultiInputManagerWin32._Instance = o.AddComponent<MultiInputManagerWin32>();
    }

    private bool IntegrityIsOK() => _Instance && this == _Instance;
    private void EnsureIntegrity()
    {
        //Debug.Log($"Checking integrity: {this.name}", this);
        if (!_Instance) _Instance = this;
        else if (!IntegrityIsOK()) Destroy(this/*.gameObject*/);
    }

    private void Awake()
    {
        //Debug.Log($"Awoken: {this.name}", this);
        EnsureIntegrity();
    }

    public void Start() {

        EnsureIntegrity();
        try
        {
            foreach(var cam in Camera.allCameras)
                Debug.Log($"{cam.name}: {cam.GetScreenRect()}");
            Display.displays[1].Activate();
        }
        catch { }

        //Cursor.lockState = CursorLockMode.Locked;
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

    [SerializeField] TMP_Text debugLabel;

    private void Update()
    {
        if (_inputReaderHandle != IntPtr.Zero)
        {
            var arr = Native.GetActiveDevicesOfType(_inputReaderHandle, Native.RIM_DEVICETYPE.MOUSE).Consume();
            foreach (var handle in arr)
            {
                try
                {
                    var state = Native.ConsumeMouseState(_inputReaderHandle, handle);
                    _getOrCreateMouse(handle).UpdateState(state);
                }
                catch(Exception e)
                {
                    Debug.LogError($"{e.Message}\n{e.StackTrace}", this);
                }
            }
        }
    }



    public void OnDestroy()
    {
        if (!IntegrityIsOK() || _inputReaderHandle == IntPtr.Zero)
        {
            Debug.Log($"Destroying self while bad integrity: {name} (env:{_inputReaderHandle})", this);
            return;
        }

        var inputReaderHandle = _inputReaderHandle;
        Debug.Log($"Stopping the input window({inputReaderHandle})", this);
        var ret = Native.StopInputInfiniteLoop(inputReaderHandle);
        Debug.Log($"Window stopping result: {ret}", this);
        Cursor.lockState = CursorLockMode.None;
    }

}

#endif