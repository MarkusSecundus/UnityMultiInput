// dllmain.cpp : Definuje vstupní bod pro aplikaci knihovny DLL.
#include "pch.h"
#include<stdlib.h>
#include<stdio.h>


static volatile HMODULE MainHModule;

LRESULT CALLBACK invisible_window_proc(HWND hwnd, UINT msg, WPARAM wParam, LPARAM lParam)
{
	switch (msg)
	{
	case WM_CLOSE:
		DestroyWindow(hwnd);
		break;
	case WM_DESTROY:
		PostQuitMessage(0);
		break;
	default:
		return DefWindowProc(hwnd, msg, wParam, lParam);
	}
	return 0;
}

static BOOL run_invisible_window(environment_t* env, HMODULE hModule) {
    static const wchar_t INVISIBLE_WINDOW_CLASS_NAME[] = L"RawInputReaderWindow";
    WNDCLASSEX wc;
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

    if (!RegisterClassEx(&wc))
    {
        DEBUGLOG(env, "Window class registration failed!");
    }
    else DEBUGLOG(env, "Window class registered successfully!");

    HWND hwnd = CreateWindowEx(
        WS_EX_CLIENTEDGE,
        INVISIBLE_WINDOW_CLASS_NAME,
        L"The title of my window",
        WS_OVERLAPPEDWINDOW,
        CW_USEDEFAULT, CW_USEDEFAULT, 240, 120,
        NULL, NULL, hModule, NULL);
    if (hwnd == NULL)
    {
        DEBUGLOG(env, "Window Creation Failed!");
        return 0;
    }
    else DEBUGLOG(env, "Window creation success!");
    UpdateWindow(hwnd);

    MSG msg;
    while (GetMessage(&msg, NULL, 0, 0) > 0)
    {
        DEBUGLOG(env, "Dispatching a message: {0}", ii(msg.message));
        TranslateMessage(&msg);
        DispatchMessage(&msg);
    }
    return msg.wParam;
}



extern "C" {
    environment_t  *InitEnvironment(
        decltype(environment_t{}.debug.format) format, decltype(environment_t{}.debug.integer) integer, decltype(environment_t{}.debug.floating) floating, decltype(environment_t{}.debug.flush) flush
    ) {
        environment_t *ret = (environment_t*) malloc(sizeof(environment_t));
        if (!ret) return NULL;
        ret->debug.format = format;
        ret->debug.integer = integer;
        ret->debug.floating = floating;
        ret->debug.flush = flush;
        DEBUGLOG(ret, "Environment successfully initialized!");
        return ret;
    }

    void DLL_EXPORT DestroyEnvironment(environment_t* env) { if (env) { DEBUGLOG(env, "Destroying the environment {0}", ii((int64_t)env)); free(env); } }



    BOOL DLL_EXPORT RunInputLoop(environment_t *env) {
        auto hModule = MainHModule;

        run_invisible_window(env, hModule);

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

