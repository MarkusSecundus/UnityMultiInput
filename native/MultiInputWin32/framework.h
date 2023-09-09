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
public:
	int32_t x = 0, y = 0;
	int32_t main_scroll = 0, horizontal_scroll = 0;
	uint32_t button_flags = 0;
};


class input_tracker_t;



struct environment_t{
	struct {
		void (__stdcall *format)(const char*);
		void (__stdcall *integer)(int64_t);
		void (__stdcall *pointer)(void*);
		void (__stdcall *floating)(double);
		void (__stdcall *cstring)(const char*);
		void (__stdcall *wstring)(const wchar_t*);
		void (__stdcall *flush)(void);
	} debug;
	input_tracker_t* input_tracker;
};

typedef HWND input_reader_handle_t;


#define DEBUGLOG(env, fformat, ...) {\
	if(env){\
		env->debug.format(fformat);\
		auto ii = env->debug.integer;\
		auto ff = env->debug.floating;\
		auto pp = env->debug.pointer;\
		auto ss = env->debug.cstring;\
		auto wss = env->debug.wstring;\
		__VA_ARGS__;\
		env->debug.flush();\
	}\
}

#define DLL_EXPORT __declspec(dllexport)

extern "C" {
	
	BOOL DLL_EXPORT RegisterInputHandle(environment_t* env);
	input_reader_handle_t DLL_EXPORT CreateInputHandle(environment_t* env);
	BOOL DLL_EXPORT RunInputInfiniteLoop(environment_t* env, input_reader_handle_t);
	BOOL DLL_EXPORT StopInputInfiniteLoop(environment_t* env, input_reader_handle_t);

	environment_t DLL_EXPORT *InitEnvironment(
		decltype(environment_t{}.debug.format),
		decltype(environment_t{}.debug.integer),
		decltype(environment_t{}.debug.pointer),
		decltype(environment_t{}.debug.floating),
		decltype(environment_t{}.debug.cstring),
		decltype(environment_t{}.debug.wstring),
		decltype(environment_t{}.debug.flush) );
	void DLL_EXPORT DestroyEnvironment(environment_t* env);


	BOOL DLL_EXPORT ReadMouseState(environment_t* env, MouseHandle mouse, mouse_state_t* out);

	native_array_t DLL_EXPORT GetAvailableDevicesOfType(environment_t* env, int deviceType);
	native_array_t DLL_EXPORT GetActiveDevicesOfType(environment_t* env, int deviceType);

	void DLL_EXPORT NativeFree(char* toFree);
}