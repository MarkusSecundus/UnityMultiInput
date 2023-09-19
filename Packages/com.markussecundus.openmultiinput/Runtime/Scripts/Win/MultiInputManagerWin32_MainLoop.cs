using System;
using System.Threading;
using UnityEngine;

using MarkusSecundus.Utils.Native;


using MouseHandle = System.IntPtr;
using KeyboardHandle = System.IntPtr;
using InputHandle = System.IntPtr;
using MarkusSecundus.Utils;

namespace MarkusSecundus.MultiInput.Windows
{
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


        public void Start()
        {
            EnsureIntegrity();
            try
            {
                foreach (var cam in Camera.allCameras)
                    Debug.Log($"{cam.name}: {cam.GetScreenRect()}");
                Display.displays[1].Activate();
            }
            catch { }

            new Thread(() =>
            {
                using var dbg = new NativeDebugEnvironment();

                var inputReaderHandle = this._inputReaderHandle = Native.InitInputHandle();
                if (inputReaderHandle == IntPtr.Zero) return;

                var ret = Native.RunInputInfiniteLoop(inputReaderHandle);
            }).Start();
        }


        private void Update()
        {
            if (_inputReaderHandle != IntPtr.Zero)
            {
                var mice = NativeUtils.GetList<MouseHandle>(add => Native.GetActiveDevicesOfType(_inputReaderHandle, Native.RIM_DEVICETYPE.MOUSE, add));
                foreach (var handle in mice)
                {
                    try
                    {
                        var state = Native.ConsumeMouseState(_inputReaderHandle, handle);
                        _getOrCreateMouse(handle).UpdateState(state);
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"{e.Message}\n{e.StackTrace}", this);
                    }
                }

                var keyboards = NativeUtils.GetList<KeyboardHandle>(add => Native.GetActiveDevicesOfType(_inputReaderHandle, Native.RIM_DEVICETYPE.KEYBOARD, add));
                foreach (var handle in keyboards)
                {
                    try
                    {
                        var events = NativeUtils.GetList<Native.KeypressDescriptor>(add => Native.ConsumeKeyboardState(_inputReaderHandle, handle, add));
                        _getOrCreateKeyboard(handle).UpdateState(events);
                    }
                    catch (Exception e)
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
                Debug.LogWarning($"Destroying self while bad integrity: {name} (env:{_inputReaderHandle})", this);
                return;
            }

            var inputReaderHandle = _inputReaderHandle;
            var ret = Native.StopInputInfiniteLoop(inputReaderHandle);
            Cursor.lockState = CursorLockMode.None;
        }

    }

#endif
}