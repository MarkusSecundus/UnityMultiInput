#pragma once
#include<vector>

struct native_array_t {
    using destructor_t = void(_stdcall*)(char*);

    native_array_t(char* begin_, int64_t length_, destructor_t destructor_) :begin(begin_), destructor(destructor_), length(length_) {}

    char* begin;
    destructor_t destructor;
    int32_t length;

    static inline native_array_t error() { return native_array_t(nullptr, -1, nullptr); }
    static inline native_array_t empty() { return native_array_t(nullptr, 0, nullptr); }


    template<typename T>
    static inline native_array_t make(std::vector<T>&& vec) {
        T* ret = (T*)malloc(vec.size() * sizeof(T));
        if (!ret)
            return native_array_t::error();

        size_t pos = 0;
        for (auto it = vec.begin(); it != vec.end();)
            ret[pos++] = *(it++);

        return native_array_t((char*)(void*)ret, vec.size(), native_array_free);
    }

private:
    static void _stdcall native_array_free(char* ptr);
};


