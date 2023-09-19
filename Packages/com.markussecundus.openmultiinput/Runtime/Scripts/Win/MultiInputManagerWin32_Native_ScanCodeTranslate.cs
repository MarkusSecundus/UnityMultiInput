using UnityEngine;


namespace MarkusSecundus.MultiInput.Windows
{

#if PLATFORM_STANDALONE_WIN
    internal partial class MultiInputManagerWin32 : MonoBehaviour, IInputProvider
    {
        internal static partial class Native
        {
            public static KeyCode NativeVirtualKeyCodeToManagedKeyCode(int virtualKeyCode, int scanCode) => (virtualKeyCode, scanCode) switch
            {
                (0x01, _) => KeyCode.Mouse0,
                (0x02, _) => KeyCode.Mouse1,
                //VK_CANCEL 	0x03 	Control-break processing
                (0x04, _) => KeyCode.Mouse2,
                (0x05, _) => KeyCode.Mouse3,
                (0x06, _) => KeyCode.Mouse4,
                //0x07 	Undefined
                (0x08, _) => KeyCode.Backspace,
                (0x09, _) => KeyCode.Tab,
                //0x0A-0B 	Reserved
                (0x0C, _) => KeyCode.Clear,
                (0x0D, _) => KeyCode.Return,
                //0x0E-0F 	Undefined
                (0x10, 54) => KeyCode.RightShift,
                (0x10, _) => KeyCode.LeftShift,
                (0x11, _) => KeyCode.LeftControl,
                (0x12, _) => KeyCode.LeftAlt,
                (0x13, _) => KeyCode.Pause,
                (0x14, _) => KeyCode.CapsLock,
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
                (0x1B, _) => KeyCode.Escape,
                /*
                VK_CONVERT 	0x1C 	IME convert
                VK_NONCONVERT 	0x1D 	IME nonconvert
                VK_ACCEPT 	0x1E 	IME accept
                VK_MODECHANGE 	0x1F 	IME mode change request
                */
                (0x20, _) => KeyCode.Space,
                (0x21, _) => KeyCode.PageUp,
                (0x22, _) => KeyCode.PageDown,
                (0x23, _) => KeyCode.End,
                (0x24, _) => KeyCode.Home,
                (0x25, _) => KeyCode.LeftArrow,
                (0x26, _) => KeyCode.UpArrow,
                (0x27, _) => KeyCode.RightArrow,
                (0x28, _) => KeyCode.DownArrow,
                //VK_SELECT 	0x29 	SELECT key
                (0x2A, _) => KeyCode.Print,
                //VK_EXECUTE 	0x2B 	EXECUTE key
                (0x2C, _) => KeyCode.Print,
                (0x2D, _) => KeyCode.Insert,
                (0x2E, _) => KeyCode.Delete,
                (0x2F, _) => KeyCode.Help,
                (0x30, _) => KeyCode.Alpha0,
                (0x31, _) => KeyCode.Alpha1,
                (0x32, _) => KeyCode.Alpha2,
                (0x33, _) => KeyCode.Alpha3,
                (0x34, _) => KeyCode.Alpha4,
                (0x35, _) => KeyCode.Alpha5,
                (0x36, _) => KeyCode.Alpha6,
                (0x37, _) => KeyCode.Alpha7,
                (0x38, _) => KeyCode.Alpha8,
                (0x39, _) => KeyCode.Alpha9,
                //0x3A-40 	Undefined
                (0x41, _) => KeyCode.A,
                (0x42, _) => KeyCode.B,
                (0x43, _) => KeyCode.C,
                (0x44, _) => KeyCode.D,
                (0x45, _) => KeyCode.E,
                (0x46, _) => KeyCode.F,
                (0x47, _) => KeyCode.G,
                (0x48, _) => KeyCode.H,
                (0x49, _) => KeyCode.I,
                (0x4A, _) => KeyCode.J,
                (0x4B, _) => KeyCode.K,
                (0x4C, _) => KeyCode.L,
                (0x4D, _) => KeyCode.M,
                (0x4E, _) => KeyCode.N,
                (0x4F, _) => KeyCode.O,
                (0x50, _) => KeyCode.P,
                (0x51, _) => KeyCode.Q,
                (0x52, _) => KeyCode.R,
                (0x53, _) => KeyCode.S,
                (0x54, _) => KeyCode.T,
                (0x55, _) => KeyCode.U,
                (0x56, _) => KeyCode.V,
                (0x57, _) => KeyCode.W,
                (0x58, _) => KeyCode.X,
                (0x59, _) => KeyCode.Y,
                (0x5A, _) => KeyCode.Z,
                (0x5B, _) => KeyCode.LeftWindows,
                (0x5C, _) => KeyCode.RightWindows,
                (0x5D, _) => KeyCode.Menu, //VK_APPS 	0x5D 	Applications key
                                           //0x5E 	Reserved
                                           //VK_SLEEP 	0x5F 	Computer Sleep key
                (0x60, _) => KeyCode.Keypad0,
                (0x61, _) => KeyCode.Keypad1,
                (0x62, _) => KeyCode.Keypad2,
                (0x63, _) => KeyCode.Keypad3,
                (0x64, _) => KeyCode.Keypad4,
                (0x65, _) => KeyCode.Keypad5,
                (0x66, _) => KeyCode.Keypad6,
                (0x67, _) => KeyCode.Keypad7,
                (0x68, _) => KeyCode.Keypad8,
                (0x69, _) => KeyCode.Keypad9,
                (0x6A, _) => KeyCode.KeypadMultiply,
                (0x6B, _) => KeyCode.KeypadPlus,
                (0x6C, _) => KeyCode.KeypadPeriod,
                (0x6D, _) => KeyCode.KeypadMinus,
                (0x6E, _) => KeyCode.KeypadPeriod,
                (0x6F, _) => KeyCode.KeypadDivide,
                (0x70, _) => KeyCode.F1,
                (0x71, _) => KeyCode.F2,
                (0x72, _) => KeyCode.F3,
                (0x73, _) => KeyCode.F4,
                (0x74, _) => KeyCode.F5,
                (0x75, _) => KeyCode.F6,
                (0x76, _) => KeyCode.F7,
                (0x77, _) => KeyCode.F8,
                (0x78, _) => KeyCode.F9,
                (0x79, _) => KeyCode.F10,
                (0x7A, _) => KeyCode.F11,
                (0x7B, _) => KeyCode.F12,
                (0x7C, _) => KeyCode.F13,
                (0x7D, _) => KeyCode.F14,
                (0x7E, _) => KeyCode.F15,
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
                (0x90, _) => KeyCode.Numlock,
                (0x91, _) => KeyCode.ScrollLock,
                //0x92 - 96     OEM specific
                //- 0x97 - 9F     Unassigned
                (0xA0, _) => KeyCode.LeftShift,
                (0xA1, _) => KeyCode.RightShift,
                (0xA2, _) => KeyCode.LeftControl,
                (0xA3, _) => KeyCode.RightControl,
                (0xA4, _) => KeyCode.LeftAlt,
                (0xA5, _) => KeyCode.RightAlt,
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
                (0xC0, _) => KeyCode.BackQuote,
                (0xBB, _) => KeyCode.Minus,
                (0xBF, _) => KeyCode.Equals,
                (0xDB, _) => KeyCode.LeftBracket,
                (0xDD, _) => KeyCode.RightBracket,
                (0xBA, _) => KeyCode.Semicolon,
                (0xDE, _) => KeyCode.Quote,
                (0xDC, _) => KeyCode.Backslash,
                (0xBD, _) => KeyCode.Slash,
                (0xBE, _) => KeyCode.Period,
                (0xBC, _) => KeyCode.Comma,

                (0xFF, _) => KeyCode.None,//KeyCode.Pause | KeyCode.Screen, 



                (0xE05B, _) => KeyCode.LeftWindows,
                (0xE012, _) => KeyCode.RightAlt,
                (0xE011, _) => KeyCode.RightControl,
                (0xE05C, _) => KeyCode.RightWindows,
                (0xE05D, _) => KeyCode.Menu,
                (0xE0FF, _) => KeyCode.Print,
                (0xE02C, _) => KeyCode.Print,
                (0xE113, _) => KeyCode.Pause,
                (0xE02D, _) => KeyCode.Insert,
                (0xE024, _) => KeyCode.Home,
                (0xE021, _) => KeyCode.PageUp,
                (0xE022, _) => KeyCode.PageDown,
                (0xE023, _) => KeyCode.End,
                (0xE02E, _) => KeyCode.Delete,
                (0xE026, _) => KeyCode.UpArrow,
                (0xE028, _) => KeyCode.DownArrow,
                (0xE025, _) => KeyCode.LeftArrow,
                (0xE027, _) => KeyCode.RightArrow,
                (0xE06F, _) => KeyCode.KeypadDivide,
                (0xE00D, _) => KeyCode.KeypadEnter,



                _ => KeyCode.None
            };
        }
    }
#endif
}