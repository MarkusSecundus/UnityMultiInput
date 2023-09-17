// dllmain.cpp : Definuje vstupní bod pro aplikaci knihovny DLL.
#include "pch.h"
#include<stdlib.h>
#include<stdio.h>
#include<memory>
#include<unordered_map>
#include<unordered_set>
#include<vector>

#include"framework.h"

static volatile HMODULE MainHModule;


struct keyboard_state_internal_t {
    using VirtualKeyCode = USHORT;

    std::unordered_set<VirtualKeyCode> pressed_down, pressed_up;
};

class input_tracker_t {
public:
    input_tracker_t(HWND window_handle_): window_handle(window_handle_){}

    mouse_state_t* find_mouse(MouseHandle h) {
        return &(mice[h]);
    }
    mutex_lock lock() { return mutex_lock(&mut); }

    void get_active_mice(Consumer<MouseHandle> pushback) {
        for (const auto& m : mice)
            pushback(m.first);
    }
    const HWND window_handle;

private:
    std::mutex mut;

    std::unordered_map<MouseHandle, mouse_state_t> mice;
};




void handle_raw_input_message(HWND hwnd, UINT inputCode, HRAWINPUT inputHandle) {
    auto tracker = (input_tracker_t*)GetWindowLongPtr(hwnd, 0);

    //DEBUGLOG("Reading raw input {1}(handle: {2}) for window {0}", pp(hwnd), ii(inputCode), pp(inputHandle));

    UINT dwSize = 0; 
    GetRawInputData(inputHandle, RID_INPUT, NULL, &dwSize, sizeof(RAWINPUTHEADER));
    auto lpb = std::make_unique<BYTE[]>(dwSize);
    if (!lpb)
    {
        DEBUGLOG("Allocation failed while handling raw input!\n");
        return;
    }

    if (GetRawInputData(inputHandle, RID_INPUT, lpb.get(), &dwSize, sizeof(RAWINPUTHEADER)) != dwSize) {
        DEBUGLOG("GetRawInputData does not return correct size !\n");
    }

    RAWINPUT* raw = (RAWINPUT*)(lpb.get());

    if (raw->header.dwType == RIM_TYPEKEYBOARD)
    {
        DEBUGLOG("Kbd({6}): make={0} Flags:{1} Reserved:{2} ExtraInformation:{3}, msg={4} VK={5} \n",
            ii(raw->data.keyboard.MakeCode),
            ii(raw->data.keyboard.Flags),
            ii(raw->data.keyboard.Reserved),
            ii(raw->data.keyboard.ExtraInformation),
            ii(raw->data.keyboard.Message),
            ii(raw->data.keyboard.VKey),
            pp(raw->header.hDevice)
        );

    }
    else if (raw->header.dwType == RIM_TYPEMOUSE)
    {
        const auto& rm(raw->data.mouse);
        /*DEBUGLOG("Mouse({8}): usFlags={0} ulButtons={1} usButtonFlags={2} usButtonData={3} ulRawButtons={4} lLastX={5} lLastY={6} ulExtraInformation={7}\r\n",
            ii(raw->data.mouse.usFlags),
            ii(raw->data.mouse.ulButtons),
            ii(raw->data.mouse.usButtonFlags),
            ii(raw->data.mouse.usButtonData),
            ii(raw->data.mouse.ulRawButtons),
            ii(raw->data.mouse.lLastX),
            ii(raw->data.mouse.lLastY),
            ii(raw->data.mouse.ulExtraInformation),
            pp(raw->header.hDevice)
        );*/

        {   auto _ = tracker->lock();

            auto m = tracker->find_mouse(raw->header.hDevice);

            if (rm.usFlags & MOUSE_MOVE_ABSOLUTE)
            {
                m->x = rm.lLastX;
                m->y = rm.lLastY;
                m->was_absolute = true;
            }
            else {
                m->x += rm.lLastX;
                m->y += rm.lLastY;
            }

            if (rm.usButtonFlags & RI_MOUSE_WHEEL) {
                m->main_scroll += (SHORT)rm.usButtonData;
            }
            if (rm.usButtonFlags & RI_MOUSE_HWHEEL) {
                m->horizontal_scroll += (SHORT)rm.usButtonData;
            }
            m->button_flags |= rm.usButtonFlags;
        }
    }
}

LRESULT CALLBACK invisible_window_proc(HWND hwnd, UINT msg, WPARAM wParam, LPARAM lParam)
{
	switch (msg)
	{
    case WM_INPUT:
    {
        auto inputCode = GET_RAWINPUT_CODE_WPARAM(wParam);
        handle_raw_input_message(hwnd, inputCode, (HRAWINPUT)lParam);
        if(inputCode == RIM_INPUT)
            goto default_label;
    }
        break;
	case WM_CLOSE:
        DestroyWindow(hwnd);
		break;
	case WM_DESTROY:
		PostQuitMessage(0);
		break;
	default:
    default_label:
		return DefWindowProc(hwnd, msg, wParam, lParam);
	}
	return 0;
}

static const wchar_t INVISIBLE_WINDOW_CLASS_NAME[] = L"RawInputReaderWindow";



static BOOL register_invisible_window_class(HMODULE hModule, const wchar_t* window_class_name) {

    WNDCLASSEX wc;

    if (GetClassInfoExW(hModule, window_class_name, &wc)) {
        DEBUGLOG("Window class '{0}' already registered!", wss(window_class_name));
        return TRUE;
    }

    wc.cbSize = sizeof(WNDCLASSEX);
    wc.style = 0;
    wc.lpfnWndProc = invisible_window_proc;
    wc.cbClsExtra = 0;
    wc.cbWndExtra = sizeof(LONG_PTR);
    wc.hInstance = hModule;
    wc.hIcon = LoadIconW(NULL, IDI_APPLICATION);
    wc.hCursor = LoadCursorW(NULL, IDC_ARROW);
    wc.hbrBackground = (HBRUSH)(COLOR_WINDOW + 1);
    wc.lpszMenuName = NULL;
    wc.lpszClassName = INVISIBLE_WINDOW_CLASS_NAME;
    wc.hIconSm = LoadIconW(NULL, IDI_APPLICATION);

    if (!RegisterClassEx(&wc)) {
        DEBUGLOG("Window class ({0}) registration failed!", wss(window_class_name));
        return FALSE;
    } 
    DEBUGLOG("Window class '{0}' registered successfully!", wss(window_class_name));
    return TRUE;
    
}

static HWND create_invisible_window(HMODULE hModule, const wchar_t* window_class_name) {
    HWND hwnd = CreateWindowEx(
        WS_EX_CLIENTEDGE,
        window_class_name,
        L"Rawinput processor window",
        WS_OVERLAPPEDWINDOW,
        CW_USEDEFAULT, CW_USEDEFAULT, 240, 120,
        NULL, NULL, hModule, NULL);
    if (hwnd == NULL)
    {
        DEBUGLOG("Window Creation Failed!");
        return NULL;
    }
    DEBUGLOG("Successfully created window {0}", pp(hwnd));
    UpdateWindow(hwnd);
    return hwnd;
}
static BOOL run_infinite_message_loop() {
    MSG msg;
    while (GetMessage(&msg, NULL, 0, 0) > 0)
    {
        //DEBUGLOG("Dispatching a message: {0}", ii(msg.message));
        TranslateMessage(&msg);
        DispatchMessage(&msg);
    }
    return msg.wParam;
}

static BOOL stop_window(HWND window) {
    DEBUGLOG("Posting the WM_CLOSE message to {0}", pp(window));
    return SendMessageW(window, WM_CLOSE, 0, 0);
}

#define HID_USAGE_PAGE__GENERIC_DESKTOP 0x0001
#define HID_USAGE_POINTER 0x0001
#define HID_USAGE_MOUSE 0x0002
#define HID_USAGE_KEYBOARD 0x0006


static BOOL register_for_raw_input(HMODULE hModule, HWND window) {
    RAWINPUTDEVICE deviceDefinitions[2];

    deviceDefinitions[0].dwFlags = 0;
    deviceDefinitions[0].usUsagePage = HID_USAGE_PAGE__GENERIC_DESKTOP;
    deviceDefinitions[0].usUsage = HID_USAGE_MOUSE;
    deviceDefinitions[0].hwndTarget = window;

    deviceDefinitions[1].dwFlags = 0;
    deviceDefinitions[1].usUsagePage = HID_USAGE_PAGE__GENERIC_DESKTOP;
    deviceDefinitions[1].usUsage = HID_USAGE_KEYBOARD;
    deviceDefinitions[1].hwndTarget = window;

    if (!RegisterRawInputDevices(deviceDefinitions, ARRAY_LENGTH(deviceDefinitions), sizeof(RAWINPUTDEVICE))) return FALSE;

    return TRUE;
}


/// <param name="rimType">Can be either RIM_TYPEMOUSE, RIM_TYPEKEYBOARD, RIM_TYPEHID</param>
static void list_all_raw_input_devices_of_type(int rimType, Consumer<HANDLE> pushback) {
    UINT numDevices = 0;
    if (GetRawInputDeviceList(NULL, &numDevices, sizeof(RAWINPUTDEVICELIST)) == -1) return;

    auto devices = std::make_unique<RAWINPUTDEVICELIST[]>(numDevices);
    if(GetRawInputDeviceList(devices.get(), &numDevices, sizeof(RAWINPUTDEVICELIST)) == -1) return;

    for (int t = 0; t < numDevices; ++t) {
        if (devices[t].dwType == rimType)
            pushback(devices[t].hDevice);
    }
}

static BOOL get_hiddevice_info(HANDLE hDevice, RID_DEVICE_INFO *deviceInfo, char** deviceName) {
    RID_DEVICE_INFO deviceInfoBuffer;
    UINT cbSize = deviceInfoBuffer.cbSize = sizeof(RID_DEVICE_INFO);
    if(GetRawInputDeviceInfoA(hDevice, RIDI_DEVICEINFO, &deviceInfoBuffer, &cbSize)==-1) return FALSE;

    GetRawInputDeviceInfoA(hDevice, RIDI_DEVICENAME, NULL, &cbSize);
    if (cbSize <= 0) return FALSE;
    char *nameBuffer = (char*)malloc(cbSize);
    if (!nameBuffer) return FALSE;
    if (GetRawInputDeviceInfoA(hDevice, RIDI_DEVICENAME, nameBuffer, &cbSize) == -1) {
        free(nameBuffer);
        return FALSE;
    };
    *deviceInfo = deviceInfoBuffer;
    *deviceName = nameBuffer;

    

    return TRUE;
}


extern "C" {
    input_tracker_t DLL_EXPORT *InitInputHandle() {
        HMODULE hModule = MainHModule;
        if(!register_invisible_window_class(hModule, INVISIBLE_WINDOW_CLASS_NAME))
            return nullptr;
        auto hwnd = create_invisible_window(hModule, INVISIBLE_WINDOW_CLASS_NAME);
        if (!hwnd) return nullptr;
        if (!register_for_raw_input(hModule, hwnd))
            DEBUGLOG("Registration for raw input failed!");
        input_tracker_t* ret = new input_tracker_t(hwnd);
        SetWindowLongPtrW(hwnd, 0, (LONG_PTR)ret);
        return ret;
    }
    BOOL DLL_EXPORT RunInputInfiniteLoop(input_tracker_t* tracker) {
        return run_infinite_message_loop();
    }
    BOOL DLL_EXPORT StopInputInfiniteLoop(input_tracker_t* tracker) {
        auto ret = stop_window(tracker->window_handle);
        delete tracker;
        return ret;
    }


    mouse_state_t DLL_EXPORT ConsumeMouseState(input_tracker_t* tracker, MouseHandle mouse) {
        auto _ = tracker->lock();

        auto state = tracker->find_mouse(mouse);
        auto ret = *state;
        *state = mouse_state_t();
        return ret;
    }

    void DLL_EXPORT GetAvailableDevicesOfType(input_tracker_t* tracker, int deviceType, Consumer<HANDLE> listPushback) {
        list_all_raw_input_devices_of_type(deviceType, listPushback);
    }
    void DLL_EXPORT GetActiveDevicesOfType(input_tracker_t* tracker, int deviceType, Consumer<HANDLE> listPushback) {
        if (deviceType == RIM_TYPEMOUSE)
            tracker->get_active_mice(listPushback);
        else if (deviceType == RIM_TYPEKEYBOARD)
            ;
    }


    BOOL DLL_EXPORT GetMouseInfo(MouseHandle mouse, mouse_info_t* out) {
        char* name;
        RID_DEVICE_INFO info;
        auto ret = get_hiddevice_info(mouse, &info, &name);
        if (!ret || info.dwType != RIM_TYPEMOUSE) return FALSE;
        
        out->id = info.mouse.dwId;
        out->numberOfButtons = info.mouse.dwNumberOfButtons;
        out->sampleRate = info.mouse.dwSampleRate;
        out->hasHorizontalWheel = !!(info.mouse.fHasHorizontalWheel);
        out->name = name;

        DEBUGLOG("Mouse info({2}): {0}'{1}'", pp(name), ss(name), ii(out->id));

        return TRUE;
    }
}





BOOL APIENTRY DllMain(HMODULE hModule, DWORD  ul_reason_for_call, LPVOID lpReserved)
{
    switch (ul_reason_for_call)
    {
        case DLL_PROCESS_ATTACH:
        case DLL_THREAD_ATTACH:
        case DLL_THREAD_DETACH:
        case DLL_PROCESS_DETACH:
        break;
    }
    MainHModule = hModule;

    return TRUE;
}

