// dllmain.cpp : Definuje vstupní bod pro aplikaci knihovny DLL.
#include "pch.h"
#include<stdlib.h>
#include<stdio.h>

#include "macro_utils.h"

static volatile HMODULE MainHModule;

static environment_t* DebugEnv;

void handle_raw_input_message(HWND hwnd, UINT inputCode, HRAWINPUT inputHandle) {
    DEBUGLOG(DebugEnv, "Reading raw input {1}(handle: {2}) for window {0}", pp(hwnd), ii(inputCode), pp(inputHandle));
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
    wc.cbWndExtra = 0;
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
    DEBUGLOG(env, "Successfully created window {0}", pp(hwnd));
    UpdateWindow(hwnd);
    return hwnd;
}
static BOOL run_infinite_message_loop(environment_t* env) {
    MSG msg;
    while (GetMessage(&msg, NULL, 0, 0) > 0)
    {
        DEBUGLOG(env, "Dispatching a message: {0}", ii(msg.message));
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
        environment_t *ret = (environment_t*) malloc(sizeof(environment_t));
        if (!ret) return NULL;
        ret->debug.format = format;
        ret->debug.integer = integer;
        ret->debug.pointer = pointer;
        ret->debug.floating = floating;
        ret->debug.cstring = cstring;
        ret->debug.wstring = wstring;
        ret->debug.flush = flush;
        DEBUGLOG(ret, "Environment {0} successfully initialized!", pp(ret));
        DebugEnv = ret;

        return ret;
    }

    void DLL_EXPORT DestroyEnvironment(environment_t* env) { if (env) { DEBUGLOG(env, "Destroying the environment {0}", ii((int64_t)env)); free(env); DebugEnv = NULL; } }


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

