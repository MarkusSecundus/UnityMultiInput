#ifndef TJNOSNDO
#define TJNOSNDO

#define DLL_EXPORT __declspec(dllexport)


extern "C" {
	int DLL_EXPORT TestFunc();
}
#endif