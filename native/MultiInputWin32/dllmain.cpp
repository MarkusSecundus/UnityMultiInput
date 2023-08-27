// dllmain.cpp : Definuje vstupní bod pro aplikaci knihovny DLL.
#include "pch.h"
#include<stdlib.h>
#include<stdio.h>

static int32_t RetVal = 43;
extern "C" {
    int DLL_EXPORT TestFunc(environment_t *env, mouse_input_frame_t *to_fill) {
        to_fill->x = RetVal;
        to_fill->y = -RetVal - 1;
        
        DEBUGLOG(env, "Filling retval with values {0} {1}", ii(to_fill->x), ff(to_fill->y));
        printf("Trying printf\n");
        puts("Trying puts\n");
        fflush(stdout);
        printf("After flush\n");
        return RetVal;
    }
    int DLL_EXPORT TestFuncRef(environment_t *env, mouse_input_frame_t &to_fill) {
        to_fill.x = RetVal;
        to_fill.y = -RetVal - 1;
        return RetVal;
    }

    void DLL_EXPORT* TestInitAll(environment_t *env, uint64_t arg) {
        static mouse_input_frame_t ss;
        ss.x = arg;
        return &ss;
    }


    environment_t DLL_EXPORT *InitEnvironment(
        decltype(environment_t{}.debug.format) format, decltype(environment_t{}.debug.integer) integer, decltype(environment_t{}.debug.floating) floating, decltype(environment_t{}.debug.flush) flush
    ) {
        environment_t *ret = (environment_t*) malloc(sizeof(environment_t));
        if (!ret) return NULL;
        ret->debug.format = format;
        ret->debug.integer = integer;
        ret->debug.floating = floating;
        ret->debug.flush = flush;
        DEBUGLOG(ret, "Input successfully initialized!");
        return ret;
    }

    void DLL_EXPORT DestroyEnvironment(environment_t* env) {if(env) free(env);}
}









BOOL APIENTRY DllMain( HMODULE hModule,
                       DWORD  ul_reason_for_call,
                       LPVOID lpReserved
                     )
{
    switch (ul_reason_for_call)
    {
    case DLL_PROCESS_ATTACH:
    case DLL_THREAD_ATTACH:
    case DLL_THREAD_DETACH:
    case DLL_PROCESS_DETACH:
        break;
    }

    RetVal = 444774;

    return TRUE;
}

