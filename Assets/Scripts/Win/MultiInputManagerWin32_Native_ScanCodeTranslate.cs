using System;
using System.Collections.Generic;
using Unity.VisualScripting.Dependencies.Sqlite;
using UnityEngine;
using static Unity.Burst.Intrinsics.X86;

#if PLATFORM_STANDALONE_WIN
internal partial class MultiInputManagerWin32 : MonoBehaviour, IInputProvider
{
	internal static partial class Native
	{
        public static KeyCode VirtualScanCodeToManagedKeyCode(int virtualKeyCode) => (virtualKeyCode&0xFF) switch
        {
            0x01 => KeyCode.Mouse0,
            0x02 => KeyCode.Mouse1,
            //VK_CANCEL 	0x03 	Control-break processing
            0x04 => KeyCode.Mouse2,
            0x05 => KeyCode.Mouse3,
            0x06 => KeyCode.Mouse4,
            //0x07 	Undefined
            0x08 => KeyCode.Backspace,
            0x09 => KeyCode.Tab,
            //0x0A-0B 	Reserved
            0x0C => KeyCode.Clear,
            0x0D => KeyCode.Return,
            //0x0E-0F 	Undefined
            0x10 => KeyCode.LeftShift,
            0x11 => KeyCode.LeftControl,
            0x12 => KeyCode.LeftAlt,
            0x13 => KeyCode.Pause,
            0x14 => KeyCode.CapsLock,
            /*
            VK_KANA 	0x15 	IME Kana mode
            VK_HANGUL 	0x15 	IME Hangul mode
            VK_IME_ON 	0x16 	IME On
            VK_JUNJA 	0x17 	IME Junja mode
            VK_FINAL 	0x18 	IME final mode
            VK_HANJA 	0x19 	IME Hanja mode
            VK_KANJI 	0x19 	IME Kanji mode
            VK_IME_OFF 	0x1A 	IME Off
            */
            0x1B => KeyCode.Escape,
            /*
            VK_CONVERT 	0x1C 	IME convert
            VK_NONCONVERT 	0x1D 	IME nonconvert
            VK_ACCEPT 	0x1E 	IME accept
            VK_MODECHANGE 	0x1F 	IME mode change request
            */
            0x20 => KeyCode.Space,
            0x21 => KeyCode.PageUp,
            0x22 => KeyCode.PageDown,
            0x23 => KeyCode.End,
            0x24 => KeyCode.Home,
            0x25 => KeyCode.LeftArrow,
            0x26 => KeyCode.UpArrow,
            0x27 => KeyCode.RightArrow,
            0x28 => KeyCode.DownArrow,
            //VK_SELECT 	0x29 	SELECT key
            0x2A => KeyCode.Print,
            //VK_EXECUTE 	0x2B 	EXECUTE key
            0x2C => KeyCode.Print,
            0x2D => KeyCode.Insert,
            0x2E => KeyCode.Delete,
            0x2F => KeyCode.Help,
            0x30 => KeyCode.Alpha0,
            0x31 => KeyCode.Alpha1,
            0x32 => KeyCode.Alpha2,
            0x33 => KeyCode.Alpha3,
            0x34 => KeyCode.Alpha4,
            0x35 => KeyCode.Alpha5,
            0x36 => KeyCode.Alpha6,
            0x37 => KeyCode.Alpha7,
            0x38 => KeyCode.Alpha8,
            0x39 => KeyCode.Alpha9,
            //0x3A-40 	Undefined
            0x41 => KeyCode.A,
            0x42 => KeyCode.B,
            0x43 => KeyCode.C,
            0x44 => KeyCode.D,
            0x45 => KeyCode.E,
            0x46 => KeyCode.F,
            0x47 => KeyCode.G,
            0x48 => KeyCode.H,
            0x49 => KeyCode.I,
            0x4A => KeyCode.J,
            0x4B => KeyCode.K,
            0x4C => KeyCode.L,
            0x4D => KeyCode.M,
            0x4E => KeyCode.N,
            0x4F => KeyCode.O,
            0x50 => KeyCode.P,
            0x51 => KeyCode.Q,
            0x52 => KeyCode.R,
            0x53 => KeyCode.S,
            0x54 => KeyCode.T,
            0x55 => KeyCode.U,
            0x56 => KeyCode.V,
            0x57 => KeyCode.W,
            0x58 => KeyCode.X,
            0x59 => KeyCode.Y,
            0x5A => KeyCode.Z,
            0x5B => KeyCode.LeftWindows,
            0x5C => KeyCode.RightWindows,
            0x5D => KeyCode.Menu, //VK_APPS 	0x5D 	Applications key
            //0x5E 	Reserved
            //VK_SLEEP 	0x5F 	Computer Sleep key
            0x60 => KeyCode.Keypad0,
            0x61 => KeyCode.Keypad1,
            0x62 => KeyCode.Keypad2,
            0x63 => KeyCode.Keypad3,
            0x64 => KeyCode.Keypad4,
            0x65 => KeyCode.Keypad5,
            0x66 => KeyCode.Keypad6,
            0x67 => KeyCode.Keypad7,
            0x68 => KeyCode.Keypad8,
            0x69 => KeyCode.Keypad9,
            0x6A => KeyCode.KeypadMultiply,
            0x6B => KeyCode.KeypadPlus,
            0x6C => KeyCode.KeypadPeriod,
            0x6D => KeyCode.KeypadMinus,
            0x6E => KeyCode.KeypadPeriod,
            0x6F => KeyCode.KeypadDivide,
            0x70 => KeyCode.F1,
            0x71 => KeyCode.F2,
            0x72 => KeyCode.F3,
            0x73 => KeyCode.F4,
            0x74 => KeyCode.F5,
            0x75 => KeyCode.F6,
            0x76 => KeyCode.F7,
            0x77 => KeyCode.F8,
            0x78 => KeyCode.F9,
            0x79 => KeyCode.F10,
            0x7A => KeyCode.F11,
            0x7B => KeyCode.F12,
            0x7C => KeyCode.F13,
            0x7D => KeyCode.F14,
            0x7E => KeyCode.F15,
            /*
            VK_F16 	0x7F 	F16 key
            VK_F17 	0x80 	F17 key
            VK_F18 	0x81 	F18 key
            VK_F19 	0x82 	F19 key
            VK_F20 	0x83 	F20 key
            VK_F21 	0x84 	F21 key
            VK_F22 	0x85 	F22 key
            VK_F23 	0x86 	F23 key
            VK_F24 	0x87 	F24 key
            */
            //- 	0x88-8F 	Unassigned
            0x90 => KeyCode.Numlock,
            0x91 => KeyCode.ScrollLock,
            //0x92 - 96     OEM specific
            //- 0x97 - 9F     Unassigned
            0xA0 => KeyCode.LeftShift,
            0xA1 => KeyCode.RightShift,
            0xA2 => KeyCode.LeftControl,
            0xA3 => KeyCode.RightControl,
            0xA4 => KeyCode.LeftAlt,
            0xA5 => KeyCode.RightAlt,
            /*
            VK_BROWSER_BACK 	0xA6 	Browser Back key
            VK_BROWSER_FORWARD 	0xA7 	Browser Forward key
            VK_BROWSER_REFRESH 	0xA8 	Browser Refresh key
            VK_BROWSER_STOP 	0xA9 	Browser Stop key
            VK_BROWSER_SEARCH 	0xAA 	Browser Search key
            VK_BROWSER_FAVORITES 	0xAB 	Browser Favorites key
            VK_BROWSER_HOME 	0xAC 	Browser Start and Home key
            VK_VOLUME_MUTE 	0xAD 	Volume Mute key
            VK_VOLUME_DOWN 	0xAE 	Volume Down key
            VK_VOLUME_UP 	0xAF 	Volume Up key
            VK_MEDIA_NEXT_TRACK 	0xB0 	Next Track key
            VK_MEDIA_PREV_TRACK 	0xB1 	Previous Track key
            VK_MEDIA_STOP 	0xB2 	Stop Media key
            VK_MEDIA_PLAY_PAUSE 	0xB3 	Play/Pause Media key
            VK_LAUNCH_MAIL 	0xB4 	Start Mail key
            VK_LAUNCH_MEDIA_SELECT 	0xB5 	Select Media key
            VK_LAUNCH_APP1 	0xB6 	Start Application 1 key
            VK_LAUNCH_APP2 	0xB7 	Start Application 2 key 
            */
            //VK_OEM_1 	0xBA 	Used for miscellaneous characters; it can vary by keyboard. For the US standard keyboard, the ;: key
            //-0xB8 - B9     Reserved
            /*
            VK_OEM_PLUS 	0xBB 	For any country/region, the + key
            VK_OEM_COMMA 	0xBC 	For any country/region, the , key
            VK_OEM_MINUS 	0xBD 	For any country/region, the - key
            VK_OEM_PERIOD 	0xBE 	For any country/region, the . key
            VK_OEM_2 	0xBF 	Used for miscellaneous characters; it can vary by keyboard. For the US standard keyboard, the /? key
            VK_OEM_3 	0xC0 	Used for miscellaneous characters; it can vary by keyboard. For the US standard keyboard, the `~ key
            - 	0xC1-D7 	Reserved
            - 	0xD8-DA 	Unassigned
            VK_OEM_4 	0xDB 	Used for miscellaneous characters; it can vary by keyboard. For the US standard keyboard, the [{ key
            VK_OEM_5 	0xDC 	Used for miscellaneous characters; it can vary by keyboard. For the US standard keyboard, the \\| key
            VK_OEM_6 	0xDD 	Used for miscellaneous characters; it can vary by keyboard. For the US standard keyboard, the ]} key
            VK_OEM_7 	0xDE 	Used for miscellaneous characters; it can vary by keyboard. For the US standard keyboard, the '" key
            VK_OEM_8 	0xDF 	Used for miscellaneous characters; it can vary by keyboard.
            - 	0xE0 	Reserved
                0xE1 	OEM specific
            VK_OEM_102 	0xE2 	The <> keys on the US standard keyboard, or the \\| key on the non-US 102-key keyboard
                0xE3-E4 	OEM specific
            VK_PROCESSKEY 	0xE5 	IME PROCESS key
                0xE6 	OEM specific
            VK_PACKET 	0xE7 	Used to pass Unicode characters as if they were keystrokes. The VK_PACKET key is the low word of a 32-bit Virtual Key value used for non-keyboard input methods. For more information, see Remark in KEYBDINPUT, SendInput, WM_KEYDOWN, and WM_KEYUP
            - 	0xE8 	Unassigned
                0xE9-F5 	OEM specific
            VK_ATTN 	0xF6 	Attn key
            VK_CRSEL 	0xF7 	CrSel key
            VK_EXSEL 	0xF8 	ExSel key
            VK_EREOF 	0xF9 	Erase EOF key
            VK_PLAY 	0xFA 	Play key
            VK_ZOOM 	0xFB 	Zoom key
            VK_NONAME 	0xFC 	Reserved
            VK_PA1 	0xFD 	PA1 key
            VK_OEM_CLEAR 	0xFE 	Clear key
            */
            0xC0 => KeyCode.BackQuote,
            0xBB => KeyCode.Minus,
            0xBF => KeyCode.Equals,
            0xDB => KeyCode.LeftBracket,
            0xDD => KeyCode.RightBracket,
            0xBA => KeyCode.Semicolon,
            0xDE => KeyCode.Quote,
            0xDC => KeyCode.Backslash,
            0xBD => KeyCode.Slash,
            0xBE => KeyCode.Period,
            0xBC => KeyCode.Comma,

            0xFF => KeyCode.None,//KeyCode.Pause | KeyCode.Screen, 


            //0x5B => KeyCode.LeftApple,

            //0x12 => KeyCode.RightAlt,
            //0x11 => KeyCode.RightAlt,
            //0x5C => KeyCode.RightMeta,
            //0x11 => KeyCode.RightControl,
            //0x5C => KeyCode.RightMeta,
            //0xD => KeyCode.KeypadEnter,



            _ => KeyCode.None
        };
    }
}
#endif