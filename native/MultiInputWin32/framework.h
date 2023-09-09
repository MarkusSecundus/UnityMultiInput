#pragma once

#define WIN32_LEAN_AND_MEAN 
#include <windows.h>
#include <stdint.h>

#include "macro_utils.h"
#include "utils.h"

using MouseHandle = HANDLE;
struct mouse_state_t {
	int32_t x = 0, y = 0;
	int32_t main_scroll = 0, horizontal_scroll = 0;
	uint32_t button_flags = 0;
};

class input_tracker_t;




#define DLL_EXPORT __declspec(dllexport)

extern "C" {
	
	input_tracker_t DLL_EXPORT *InitInputHandle();
	BOOL DLL_EXPORT RunInputInfiniteLoop(input_tracker_t*);
	BOOL DLL_EXPORT StopInputHandler(input_tracker_t*);


	BOOL DLL_EXPORT ReadMouseState(input_tracker_t* tracker, MouseHandle mouse, mouse_state_t* out);

	native_array_t DLL_EXPORT GetAvailableDevicesOfType(input_tracker_t* tracker, int deviceType);
	native_array_t DLL_EXPORT GetActiveDevicesOfType(input_tracker_t* tracker, int deviceType);
}