#include "pch.h"

#include "debug.h"


debug_env_t* _DebugEnv = nullptr;


extern "C" {
    void* InitDebug(
        decltype(debug_env_t{}.format) format,
        decltype(debug_env_t{}.integer) integer,
        decltype(debug_env_t{}.pointer) pointer,
        decltype(debug_env_t{}.floating) floating,
        decltype(debug_env_t{}.cstring) cstring,
        decltype(debug_env_t{}.wstring) wstring,
        decltype(debug_env_t{}.flush) flush
    ) {
        debug_env_t* ret = new debug_env_t();
        if (!ret) return NULL;
        _DebugEnv = ret;
        ret->format = format;
        ret->integer = integer;
        ret->pointer = pointer;
        ret->floating = floating;
        ret->cstring = cstring;
        ret->wstring = wstring;
        ret->flush = flush;

        DEBUGLOG("Environment {0} successfully initialized!", pp(ret));

        return ret;
    }

    void DLL_EXPORT DestroyDebug() {
        if (_DebugEnv) {
            DEBUGLOG("Destroying the environment {0}", pp(_DebugEnv));
            delete _DebugEnv;
            _DebugEnv = nullptr;
        }
    }
}