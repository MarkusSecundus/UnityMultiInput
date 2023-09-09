#include "pch.h"

#include<stdlib.h>
#include "utils.h"


void _stdcall native_array_t::native_array_free(char* ptr) {
    free(ptr);
}