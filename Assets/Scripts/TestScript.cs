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
using EnvHandle = System.IntPtr;

public enum RIM_DEVICETYPE : int
{
    MOUSE=0, KEYBOARD=1, HID=2
}

public class TestScript : MonoBehaviour
{

    [DllImport("MultiInputWin32.dll")]
    public static extern int RegisterInputHandle(EnvHandle env);
    [DllImport("MultiInputWin32.dll")]
    public static extern IntPtr CreateInputHandle(EnvHandle env);
    [DllImport("MultiInputWin32.dll")]
    public static extern int RunInputInfiniteLoop(EnvHandle env, MouseHandle inputReaderHandle);
    [DllImport("MultiInputWin32.dll")]
    public static extern int StopInputInfiniteLoop(EnvHandle env, MouseHandle inputReaderHandle);



    [DllImport("MultiInputWin32.dll")]
    public static extern EnvHandle InitEnvironment(NativeAction<string> format, NativeAction<long> integer, NativeAction<IntPtr> pointer, NativeAction<double> floating, NativeAction<string> cstring, NativeWstringAction wstring, NativeAction flush);
    [DllImport("MultiInputWin32.dll")]
    public static extern void DestroyEnvironment(EnvHandle env);


    
    [DllImport("MultiInputWin32.dll")]
    public static extern int ReadMouseState(EnvHandle env, MouseHandle mouseHandle, out MouseInputFrame ret);

    
    [DllImport("MultiInputWin32.dll")]
    public static extern NativeArray<MouseHandle> GetAvailableDevicesOfType(EnvHandle env, RIM_DEVICETYPE deviceType);
    [DllImport("MultiInputWin32.dll")]
    public static extern NativeArray<MouseHandle> GetActiveDevicesOfType(EnvHandle env, RIM_DEVICETYPE deviceType);


    public TMP_Text debugPrototype;

    Dictionary<MouseHandle, TMP_Text> debuggersForMice = new();

    class NativeDebugManager : IDisposable
    {
        

        public bool IsSilent = false;
        NativeAction<string> format;
        NativeAction<long> integer;
        NativeAction<IntPtr> pointer;
        NativeAction<double> floating;
        NativeAction<string> cstring;
        NativeWstringAction wstring;
        NativeAction flush, silentFlush;
        string formatString = "";
        List<object> args = new List<object>();
        public readonly IntPtr Env;
        public NativeDebugManager()
        {
            Env = InitEnvironment(
                 format = s => formatString = s,
                 integer = i => args.Add(i),
                 pointer = p => args.Add(p),
                 floating = d => args.Add(d),
                 cstring = s => args.Add(s),
                 wstring = s => args.Add(s),
                 flush = () =>
                 {
                     if (!IsSilent)
                     {
                         try
                         {
                             Debug.Log("native: " + string.Format(formatString, args.ToArray()));
                         }
                         catch
                         {
                             Debug.LogError($"Error during native debug... '{formatString}', args: [{args.MakeString()}]");
                         }
                     }
                     formatString = "";
                     args.Clear();
                 }
            );
        }

        public void Dispose() => DestroyEnvironment(Env);
    }
    [StructLayout(LayoutKind.Sequential)]
    public struct MouseInputFrame
    {
        public int x, y;
        public int main_scroll, horizontal_scroll;
        public ButtonFlags button_flags;

        public override string ToString() => $"<({x}, {y})-sc({main_scroll}::{horizontal_scroll})-fl({(uint)button_flags:x})>";

        public enum ButtonFlags : uint
        {
            RI_MOUSE_LEFT_BUTTON_DOWN = 0x0001,
            RI_MOUSE_LEFT_BUTTON_UP = 0x0002,
            RI_MOUSE_RIGHT_BUTTON_DOWN = 0x0004,
            RI_MOUSE_RIGHT_BUTTON_UP = 0x0008,
            RI_MOUSE_MIDDLE_BUTTON_DOWN = 0x0010,
            RI_MOUSE_MIDDLE_BUTTON_UP = 0x0020,
            RI_MOUSE_BUTTON_4_DOWN = 0x0040,
            RI_MOUSE_BUTTON_4_UP = 0x0080,
            RI_MOUSE_BUTTON_5_DOWN = 0x0100,
            RI_MOUSE_BUTTON_5_UP = 0x0200
        }
    }



    volatile IntPtr inputReaderHandle = IntPtr.Zero;
    volatile NativeDebugManager dbg = null;

    public void Start()
    {

        new Thread(() => 
        {
            dbg = new();
            Debug.Log("Starting new thread for win32 coop");

            if (RegisterInputHandle(dbg.Env) != 1) return;
            var inputReaderHandle = this.inputReaderHandle = CreateInputHandle(dbg.Env);
            Debug.Log($"Created input window({inputReaderHandle})");

            dbg.IsSilent = true;
            var ret = RunInputInfiniteLoop(dbg.Env, inputReaderHandle);
            dbg.IsSilent = false;

            Debug.Log($"Ending win32 coop thread (ret: {ret})");
        }).Start();
         
        StartCoroutine(periodicPrintout());
        IEnumerator periodicPrintout()
        {
            while (true)
            {
                if(dbg!= null)
                {
                    var arr = GetAvailableDevicesOfType(dbg.Env, RIM_DEVICETYPE.MOUSE).Consume();
                    Debug.Log($"Available mouse devices({arr.Length}): [{arr.MakeString()}]");
                    arr = GetActiveDevicesOfType(dbg.Env, RIM_DEVICETYPE.MOUSE).Consume();
                    Debug.Log($"Active mouse devices({arr.Length}): [{arr.MakeString()}]...");
                    foreach(var handle in arr)
                    {
                        var success = ReadMouseState(dbg.Env, handle, out var state);
                        Debug.Log($"{handle}->{success}...{state}");
                        getLabel(handle).text = $"{handle}...{state}";
                    }
                    
                }
                yield return new WaitForSeconds(0.25f);

                TMP_Text getLabel(MouseHandle h)
                {
                    if (debuggersForMice.TryGetValue(h, out var ret)) return ret;
                    ret = Instantiate(debugPrototype);
                    ret.transform.SetParent(debugPrototype.transform.parent);
                    ret.gameObject.SetActive(true);

                    ret.rectTransform.position = new Vector2(UnityEngine.Random.Range(100, Camera.main.scaledPixelWidth/2), UnityEngine.Random.Range(100, Camera.main.scaledPixelHeight/2));

                    debuggersForMice[h] = ret;
                    return ret;
                }
            }
        }
    }


    public void OnDestroy()
    {
        dbg.IsSilent = false;
        var inputReaderHandle = this.inputReaderHandle;
        Debug.Log($"Stopping the input window({inputReaderHandle})");
        var ret = StopInputInfiniteLoop(dbg.Env, inputReaderHandle);
        Debug.Log($"Window stopping result: {ret}");
        dbg.Dispose(); dbg = null;
    }
}
