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
using KeyboardHandle = System.IntPtr;
using InputHandle = System.IntPtr;
using UnityEngine.UI;
using System.Net;
using static MultiInputManagerWin32;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine.EventSystems;
using System.IO;

#if PLATFORM_STANDALONE_WIN
internal partial class MultiInputManagerWin32 : MonoBehaviour
{
    static MultiInputConfigWin32 _config;
    static MultiInputConfigWin32 _Config => _config ? _config : _config = Resources.Load<MultiInputConfigWin32>(ResourcePaths.ConfigWin32);

    static MultiInputManagerWin32 _Instance { get; set; }

    volatile IntPtr _inputReaderHandle = IntPtr.Zero;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void AutocreateTheInputEnvironment()
    {
        if (!_Config.IsMultiInputEnabled) return;
        var o = new GameObject("[MultiInput_Win32]");
        GameObject.DontDestroyOnLoad(o);
        MultiInputManagerWin32._Instance = o.AddComponent<MultiInputManagerWin32>();
    }

    private bool IntegrityIsOK() => _Config.IsMultiInputEnabled && _Instance && this == _Instance;
    private void EnsureIntegrity()
    {
        if (!_Config.IsMultiInputEnabled) Destroy(this);
        if (!_Instance) _Instance = this;
        else if (!IntegrityIsOK()) Destroy(this/*.gameObject*/);
    }

    private void Awake()
    {
        EnsureIntegrity();
    }


    TextWriter wrt;
    public void Start() 
    {
        wrt = new StreamWriter("input_test.txt");
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
            var mice = NativeUtils.GetList<MouseHandle>(add=>Native.GetActiveDevicesOfType(_inputReaderHandle, Native.RIM_DEVICETYPE.MOUSE, add));
            foreach (var handle in mice)
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

            var keyboards = NativeUtils.GetList<KeyboardHandle>(add => Native.GetActiveDevicesOfType(_inputReaderHandle, Native.RIM_DEVICETYPE.KEYBOARD, add));
            foreach(var handle in keyboards)
            {
                try
                {
                    __keyboardTest(handle);
                    //var events = NativeUtils.GetList<Native.KeypressDescriptor>(add => Native.ConsumeKeyboardState(_inputReaderHandle, handle, add));
                    //_getOrCreateKeyboard(handle).UpdateState(events);
                }
                catch (Exception e)
                {
                    Debug.LogError($"{e.Message}\n{e.StackTrace}", this);
                }
            }
        }
    }

    private void __keyboardTest(KeyboardHandle handle)
    {
        var events = NativeUtils.GetList<Native.KeypressDescriptor>(add => Native.ConsumeKeyboardState(_inputReaderHandle, handle, add));

        if (events.Count > 1) Debug.Log($"Multiple events ({events.Count})...");
        foreach (var ev in events)
        {
            if (ev.PressState == Native.KeypressDescriptor.State.PRESS_UP) continue;
            var (vkey, scan) = (ev.VirtualKeyCode, ev.ScanCode);
            var supposedCode = Native.NativeVirtualKeyCodeToManagedKeyCode(vkey, scan);
            bool didFind = false;
            foreach (var c in Enum.GetValues(typeof(KeyCode)).Cast<KeyCode>())
                if (Input.GetKey(c))
                {
                    Debug.Log($"0x{vkey:X} => {c}({supposedCode})      <{ev.ScanCode}>");
                    if (c != supposedCode)
                        wrt.WriteLine($"0x{vkey:X} => {nameof(KeyCode)}.{c},");
                    didFind = true;
                }
            if (!didFind) Debug.Log($"No keypress found for native {vkey}!");
        }
    }


    public void OnDestroy()
    {
        wrt.Dispose();
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