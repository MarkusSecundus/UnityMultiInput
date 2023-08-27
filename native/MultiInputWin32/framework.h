﻿#pragma once

#define WIN32_LEAN_AND_MEAN 
#include <windows.h>
#include <stdint.h>

typedef struct {
	int32_t x;
	int32_t y;
} mouse_input_frame_t;

typedef struct {
	struct {
		void (*format)(const char*);
		void (*integer)(int64_t);
		void (*floating)(double);
		void (*flush)(void);
	} debug;
} environment_t;


#define DEBUGLOG(env, fformat, ...) {\
	env->debug.format(fformat);\
	auto ii = env->debug.integer;\
	auto ff = env->debug.floating;\
	__VA_ARGS__;\
	env->debug.flush();\
}

#define DLL_EXPORT __declspec(dllexport)

extern "C" {
	int DLL_EXPORT TestFunc(environment_t *env, mouse_input_frame_t *to_fill);
	int DLL_EXPORT TestFuncRef(environment_t *env, mouse_input_frame_t &to_fill);
	void DLL_EXPORT* TestInitAll(environment_t *env, uint64_t arg);

	environment_t DLL_EXPORT *InitEnvironment(decltype(environment_t{}.debug.format),decltype(environment_t{}.debug.integer),decltype(environment_t{}.debug.floating),decltype(environment_t{}.debug.flush) );
}