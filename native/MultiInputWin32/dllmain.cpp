// dllmain.cpp : Definuje vstupní bod pro aplikaci knihovny DLL.
#include "pch.h"
#include<stdlib.h>
#include<stdio.h>
#include<memory>
#include<unordered_map>
#include<mutex>
#include<vector>
#include<optional>

#include "macro_utils.h"

static volatile HMODULE MainHModule;
debug_env_t* _DebugEnv = nullptr;


struct mutex_lock {
    mutex_lock(std::mutex *mutex) :m(mutex) { m->lock(); }
    ~mutex_lock() { m->unlock(); }
private:
    std::mutex *m;
};




class input_tracker_t {
public:
    input_tracker_t(HWND window_handle_): window_handle(window_handle_){}

    mouse_state_t* find_mouse(MouseHandle h) {
        return &(mice[h]);
    }
    mutex_lock lock() { return mutex_lock(&mut); }

    std::vector<MouseHandle> get_active_mice() {
        std::vector<MouseHandle> ret;
        for (const auto& m : mice) {
            ret.emplace_back(m.first);
        }
        return ret;
    }
    const HWND window_handle;

private:
    std::mutex mut;

    std::unordered_map<MouseHandle, mouse_state_t> mice;
};



void _stdcall native_array_free(char* ptr) {
    free(ptr);
}
template<typename T>
native_array_t to_native_array(std::vector<T>&& vec) {
    T* ret = (T*)malloc(vec.size()*sizeof(T));
    if (!ret) 
        return native_array_t::error();
    
    size_t pos = 0;
    for (auto it = vec.begin(); it != vec.end();)
        ret[pos++] = *(it++);

    return native_array_t((char*)(void*)ret, vec.size(), native_array_free);
}


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
        DEBUGLOG("Mouse({8}): usFlags={0} ulButtons={1} usButtonFlags={2} usButtonData={3} ulRawButtons={4} lLastX={5} lLastY={6} ulExtraInformation={7}\r\n",
            ii(raw->data.mouse.usFlags),
            ii(raw->data.mouse.ulButtons),
            ii(raw->data.mouse.usButtonFlags),
            ii(raw->data.mouse.usButtonData),
            ii(raw->data.mouse.ulRawButtons),
            ii(raw->data.mouse.lLastX),
            ii(raw->data.mouse.lLastY),
            ii(raw->data.mouse.ulExtraInformation),
            pp(raw->header.hDevice)
        );

        {   auto _ = tracker->lock();

            auto m = tracker->find_mouse(raw->header.hDevice);

            if (rm.usFlags & MOUSE_MOVE_ABSOLUTE)
            {
                m->x = rm.lLastX;
                m->y = rm.lLastY;
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




static BOOL register_for_raw_input(HMODULE hModule, HWND window) {
#if 1
    RAWINPUTDEVICE deviceDefinitions[2];

    deviceDefinitions[0].dwFlags = 0;
    deviceDefinitions[0].usUsagePage = 0x0001;
    deviceDefinitions[0].usUsage = 0x0001;
    deviceDefinitions[0].hwndTarget = window;

    deviceDefinitions[1].dwFlags = 0;
    deviceDefinitions[1].usUsagePage = 0x0001;
    deviceDefinitions[1].usUsage = 0x0002;
    deviceDefinitions[1].hwndTarget = window;
#else
    RAWINPUTDEVICE deviceDefinitions[1];

    deviceDefinitions[0].dwFlags = RIDEV_PAGEONLY;// | RIDEV_EXINPUTSINK;
    deviceDefinitions[0].usUsagePage = 0x0001;
    deviceDefinitions[0].usUsage = 0;
    deviceDefinitions[0].hwndTarget = window;

#endif

    if (!RegisterRawInputDevices(deviceDefinitions, ARRAY_LENGTH(deviceDefinitions), sizeof(RAWINPUTDEVICE))) return FALSE;

    return TRUE;
}


/// <param name="rimType">Can be either RIM_TYPEMOUSE, RIM_TYPEKEYBOARD, RIM_TYPEHID</param>
static std::vector<HANDLE> list_all_raw_input_devices_of_type(int rimType) {
    UINT numDevices = 0;
    if (GetRawInputDeviceList(NULL, &numDevices, sizeof(RAWINPUTDEVICELIST)) == -1) return std::vector<HANDLE>{};

    auto devices = std::make_unique<RAWINPUTDEVICELIST[]>(numDevices);
    if(GetRawInputDeviceList(devices.get(), &numDevices, sizeof(RAWINPUTDEVICELIST)) == -1) return std::vector<HANDLE>{};

    std::vector<MouseHandle> ret;
    for (int t = 0; t < numDevices; ++t) {
        if (devices[t].dwType == rimType)
            ret.emplace_back(devices[t].hDevice);
    }
    
    return ret;
}


extern "C" {
    void  *InitDebug(
        decltype(debug_env_t{}.format) format,
        decltype(debug_env_t{}.integer) integer,
        decltype(debug_env_t{}.pointer) pointer,
        decltype(debug_env_t{}.floating) floating,
        decltype(debug_env_t{}.cstring) cstring,
        decltype(debug_env_t{}.wstring) wstring,
        decltype(debug_env_t{}.flush) flush
    ) {
        debug_env_t *ret = new debug_env_t();
        if (!ret) return NULL;
        _DebugEnv = ret;
        ret->format = format;
        ret->integer = integer;
        ret->pointer = pointer;
        ret->floating = floating;
        ret->cstring = cstring;
        ret->wstring = wstring;
        ret->flush = flush;
        
        DEBUGLOG("Environment {0} successfully initialized!", pp(ret));

        return ret;
    }

    void DLL_EXPORT DestroyDebug() { 
        if (_DebugEnv) {
            DEBUGLOG("Destroying the environment {0}", pp(_DebugEnv)); 
            delete _DebugEnv;
            _DebugEnv = nullptr;
        } 
    }


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


    BOOL DLL_EXPORT ReadMouseState(input_tracker_t* tracker, MouseHandle mouse, mouse_state_t* out) {
        auto _ = tracker->lock();

        auto state = tracker->find_mouse(mouse);
        *out = *state;
        state->button_flags = 0;
        return TRUE;
    }

    native_array_t DLL_EXPORT GetAvailableDevicesOfType(input_tracker_t* tracker, int deviceType) {
        return to_native_array(list_all_raw_input_devices_of_type(deviceType));
    }
    native_array_t DLL_EXPORT GetActiveDevicesOfType(input_tracker_t* tracker, int deviceType) {
        if (deviceType == RIM_TYPEMOUSE)
            return to_native_array(tracker->get_active_mice());
        else
            return native_array_t::empty();
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

