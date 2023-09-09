#pragma once

#define WIN32_LEAN_AND_MEAN 
#include <windows.h>
#include <stdint.h>


struct native_array_t {
	using destructor_t = void(_stdcall*)(char*);

	native_array_t(char* begin_, int64_t length_, destructor_t destructor_):begin(begin_), destructor(destructor_), length(length_) {}
	char* begin;
	destructor_t destructor;
	int32_t length;

	static inline native_array_t error() { return native_array_t(nullptr, -1, nullptr); }
	static inline native_array_t empty() { return native_array_t(nullptr, 0, nullptr); }
};

using MouseHandle = HANDLE;
struct mouse_state_t {
	int32_t x = 0, y = 0;
	int32_t main_scroll = 0, horizontal_scroll = 0;
	uint32_t button_flags = 0;
};

class input_tracker_t;



struct debug_env_t{
	void(__stdcall* format)(const char*);
	void(__stdcall* integer)(int64_t);
	void(__stdcall* pointer)(void*);
	void(__stdcall* floating)(double);
	void(__stdcall* cstring)(const char*);
	void(__stdcall* wstring)(const wchar_t*);
	void(__stdcall* flush)(void);
};
extern debug_env_t* _DebugEnv;

#define DEBUGLOG(fformat, ...) {\
	if(_DebugEnv){\
		_DebugEnv->format(fformat);\
		auto ii = _DebugEnv->integer;\
		auto ff = _DebugEnv->floating;\
		auto pp = _DebugEnv->pointer;\
		auto ss = _DebugEnv->cstring;\
		auto wss = _DebugEnv->wstring;\
		__VA_ARGS__;\
		_DebugEnv->flush();\
	}\
}

#define DLL_EXPORT __declspec(dllexport)

extern "C" {
	
	input_tracker_t DLL_EXPORT *InitInputHandle();
	BOOL DLL_EXPORT RunInputInfiniteLoop(input_tracker_t*);
	BOOL DLL_EXPORT StopInputHandler(input_tracker_t*);

	void DLL_EXPORT *InitDebug(
		decltype(debug_env_t{}.format),
		decltype(debug_env_t{}.integer),
		decltype(debug_env_t{}.pointer),
		decltype(debug_env_t{}.floating),
		decltype(debug_env_t{}.cstring),
		decltype(debug_env_t{}.wstring),
		decltype(debug_env_t{}.flush) );
	void DLL_EXPORT DestroyDebug();


	BOOL DLL_EXPORT ReadMouseState(input_tracker_t* tracker, MouseHandle mouse, mouse_state_t* out);

	native_array_t DLL_EXPORT GetAvailableDevicesOfType(input_tracker_t* tracker, int deviceType);
	native_array_t DLL_EXPORT GetActiveDevicesOfType(input_tracker_t* tracker, int deviceType);
}