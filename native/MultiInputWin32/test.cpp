#include "test.h"

extern "C" {
	int DLL_EXPORT TestFunc() {
		return 54334;
	}
}