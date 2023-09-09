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



struct mutex_lock {
    mutex_lock(std::mutex *mutex) :m(mutex) { m->lock(); }
    ~mutex_lock() { m->unlock(); }
private:
    std::mutex *m;
};




class input_tracker_t {
public:
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

private:
    std::mutex mut;

    std::unordered_map<MouseHandle, mouse_state_t> mice;
};

volatile input_tracker_t* InputTracker;


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
    auto env = (environment_t*)GetWindowLongPtrW(hwnd, 0);
    auto tracker = env->input_tracker;

    //DEBUGLOG(env, "Reading raw input {1}(handle: {2}) for window {0}", pp(hwnd), ii(inputCode), pp(inputHandle));

    UINT dwSize = 0; 
    GetRawInputData(inputHandle, RID_INPUT, NULL, &dwSize, sizeof(RAWINPUTHEADER));
    auto lpb = std::make_unique<BYTE[]>(dwSize);
    if (!lpb)
    {
        DEBUGLOG(env, "Allocation failed while handling raw input!\n");
        return;
    }

    if (GetRawInputData(inputHandle, RID_INPUT, lpb.get(), &dwSize, sizeof(RAWINPUTHEADER)) != dwSize) {
        DEBUGLOG(env, "GetRawInputData does not return correct size !\n");
    }

    RAWINPUT* raw = (RAWINPUT*)(lpb.get());

    if (raw->header.dwType == RIM_TYPEKEYBOARD)
    {
        DEBUGLOG(env, " Kbd({6}): make={0} Flags:{1} Reserved:{2} ExtraInformation:{3}, msg={4} VK={5} \n",
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
        DEBUGLOG(env, "Mouse({8}): usFlags={0} ulButtons={1} usButtonFlags={2} usButtonData={3} ulRawButtons={4} lLastX={5} lLastY={6} ulExtraInformation={7}\r\n",
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

static BOOL register_invisible_window_class(environment_t* env, HMODULE hModule, const wchar_t* window_class_name) {

    WNDCLASSEX wc;

    if (GetClassInfoExW(hModule, window_class_name, &wc)) {
        DEBUGLOG(env, "Window class '{0}' already registered!", wss(window_class_name));
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
        DEBUGLOG(env, "Window class ({0}) registration failed!", wss(window_class_name));
        return FALSE;
    } 
    DEBUGLOG(env, "Window class '{0}' registered successfully!", wss(window_class_name));
    return TRUE;
    
}

static HWND create_invisible_window(environment_t* env, HMODULE hModule, const wchar_t* window_class_name) {
    HWND hwnd = CreateWindowEx(
        WS_EX_CLIENTEDGE,
        window_class_name,
        L"The title of my window",
        WS_OVERLAPPEDWINDOW,
        CW_USEDEFAULT, CW_USEDEFAULT, 240, 120,
        NULL, NULL, hModule, NULL);
    if (hwnd == NULL)
    {
        DEBUGLOG(env, "Window Creation Failed!");
        return NULL;
    }
    SetWindowLongPtrW(hwnd, 0, (LONG_PTR)env);
    DEBUGLOG(env, "Successfully created window {0}", pp(hwnd));
    UpdateWindow(hwnd);
    return hwnd;
}
static BOOL run_infinite_message_loop(environment_t* env) {
    MSG msg;
    while (GetMessage(&msg, NULL, 0, 0) > 0)
    {
        //DEBUGLOG(env, "Dispatching a message: {0}", ii(msg.message));
        TranslateMessage(&msg);
        DispatchMessage(&msg);
    }
    return msg.wParam;
}

static BOOL stop_window(environment_t* env, HWND window) {
    DEBUGLOG(env, "Posting the WM_CLOSE message to {0}", pp(window));
    return SendMessageW(window, WM_CLOSE, 0, 0);
}


static BOOL register_for_raw_input(environment_t* env, HMODULE hModule, HWND window) {
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
static std::vector<HANDLE> list_all_raw_input_devices_of_type(environment_t* env, int rimType) {
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
    environment_t  *InitEnvironment(
        decltype(environment_t{}.debug.format) format,
        decltype(environment_t{}.debug.integer) integer,
        decltype(environment_t{}.debug.pointer) pointer,
        decltype(environment_t{}.debug.floating) floating,
        decltype(environment_t{}.debug.cstring) cstring,
        decltype(environment_t{}.debug.wstring) wstring,
        decltype(environment_t{}.debug.flush) flush
    ) {
        environment_t *ret = new environment_t();
        if (!ret) return NULL;
        ret->debug.format = format;
        ret->debug.integer = integer;
        ret->debug.pointer = pointer;
        ret->debug.floating = floating;
        ret->debug.cstring = cstring;
        ret->debug.wstring = wstring;
        ret->debug.flush = flush;
        
        ret->input_tracker = new input_tracker_t();

        DEBUGLOG(ret, "Environment {0} successfully initialized!", pp(ret));

        return ret;
    }

    void DLL_EXPORT DestroyEnvironment(environment_t* env) { if (env) {
        DEBUGLOG(env, "Destroying the environment {0}", ii((int64_t)env)); 
        delete env->input_tracker;
        delete env; 
    } }


    BOOL DLL_EXPORT RegisterInputHandle(environment_t* env) {
        return TRUE;
    }
    input_reader_handle_t DLL_EXPORT CreateInputHandle(environment_t* env) {
        HMODULE hModule = MainHModule;
        if(!register_invisible_window_class(env, hModule, INVISIBLE_WINDOW_CLASS_NAME))
            return NULL;
        auto ret = create_invisible_window(env, hModule, INVISIBLE_WINDOW_CLASS_NAME);
        if (!ret) return NULL;
        if (!register_for_raw_input(env, hModule, ret))
            DEBUGLOG(env, "Registration for raw input failed!");
        return ret;
    }
    BOOL DLL_EXPORT RunInputInfiniteLoop(environment_t* env, input_reader_handle_t hwnd) {
        return run_infinite_message_loop(env);
    }
    BOOL DLL_EXPORT StopInputInfiniteLoop(environment_t* env, input_reader_handle_t hwnd) {
        return stop_window(env, hwnd);
    }


    BOOL DLL_EXPORT ReadMouseState(environment_t* env, MouseHandle mouse, mouse_state_t* out) {
        auto tracker = env->input_tracker;
        auto _ = tracker->lock();

        auto state = tracker->find_mouse(mouse);
        *out = *state;
        state->button_flags = 0;
        return TRUE;
    }

    native_array_t DLL_EXPORT GetAvailableDevicesOfType(environment_t* env, int deviceType) {
        return to_native_array(list_all_raw_input_devices_of_type(env, deviceType));
    }
    native_array_t DLL_EXPORT GetActiveDevicesOfType(environment_t* env, int deviceType) {
        if (deviceType == RIM_TYPEMOUSE)
            return to_native_array(env->input_tracker->get_active_mice());
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

