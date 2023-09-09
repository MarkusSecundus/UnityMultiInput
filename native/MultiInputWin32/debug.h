#pragma once
#include <stdint.h>
#include "macro_utils.h"

struct debug_env_t {
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


extern "C" {
	void DLL_EXPORT* InitDebug(
		decltype(debug_env_t{}.format),
		decltype(debug_env_t{}.integer),
		decltype(debug_env_t{}.pointer),
		decltype(debug_env_t{}.floating),
		decltype(debug_env_t{}.cstring),
		decltype(debug_env_t{}.wstring),
		decltype(debug_env_t{}.flush));
	void DLL_EXPORT DestroyDebug();
}