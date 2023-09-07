#pragma once

#define WIN32_LEAN_AND_MEAN 
#include <windows.h>
#include <stdint.h>
 

typedef struct {
	int32_t x;
	int32_t y;
} mouse_input_frame_t;

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


}