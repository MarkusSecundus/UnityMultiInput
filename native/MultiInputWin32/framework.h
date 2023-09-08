#pragma once

#define WIN32_LEAN_AND_MEAN 
#include <windows.h>
#include <stdint.h>


struct native_array_t {
	native_array_t(char* begin_, int64_t length_):begin(begin_), length(length_){}
	char* begin;
	int32_t length;
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
		void (*format)(const char*);
		void (*integer)(int64_t);
		void (*pointer)(void*);
		void (*floating)(double);
		void (*cstring)(const char*);
		void (*wstring)(const wchar_t*);
		void (*flush)(void);
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