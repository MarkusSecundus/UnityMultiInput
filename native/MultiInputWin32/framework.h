#pragma once

#define WIN32_LEAN_AND_MEAN 
#include <windows.h>
#include <stdint.h>

#include "macro_utils.h"
#include "utils.h"

using MouseHandle = HANDLE;
using KeyboardHandle = HANDLE;
struct mouse_state_t {
	int32_t x = 0, y = 0;
	int32_t main_scroll = 0, horizontal_scroll = 0;
	uint32_t button_flags = 0;
	bool was_absolute = false;
};

struct keypress_descriptor_t {
	enum class state_t : int8_t{
		PRESS_DOWN=0, PRESS_UP=1
	};
	int32_t scan_code;
	int32_t virtual_key_code;
	int32_t text_value;
	state_t press_state;
};

struct mouse_info_t {
	int32_t id;
	int32_t numberOfButtons;
	int32_t sampleRate;
	bool hasHorizontalWheel;
	const char* name;
};

class input_tracker_t;





extern "C" {
	
	input_tracker_t DLL_EXPORT *InitInputHandle();
	BOOL DLL_EXPORT RunInputInfiniteLoop(input_tracker_t*);
	BOOL DLL_EXPORT StopInputHandler(input_tracker_t*);


	mouse_state_t DLL_EXPORT ConsumeMouseState(input_tracker_t* tracker, MouseHandle mouse);
	void DLL_EXPORT ConsumeKeyboardState(input_tracker_t *tracker, KeyboardHandle keyboard, Consumer<keypress_descriptor_t> listPushback);

	void DLL_EXPORT GetAvailableDevicesOfType(input_tracker_t* tracker, int deviceType, Consumer<HANDLE> listPushback);
	void DLL_EXPORT GetActiveDevicesOfType(input_tracker_t* tracker, int deviceType, Consumer<HANDLE> listPushback);

	BOOL DLL_EXPORT GetMouseInfo(MouseHandle mouse, mouse_info_t* out);
}