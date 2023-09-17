#pragma once
#include<vector>
#include<mutex>

struct native_array_t {
    using destructor_t = void(_stdcall*)(char*);

private:
    native_array_t(char* begin_, int64_t length_, destructor_t destructor_) :begin(begin_), destructor(destructor_), length(length_) {}
public:
    char* begin;
    destructor_t destructor;
    int32_t length;

    inline void destroy_self() { destructor(begin); begin = NULL; length = 0; }

    static inline native_array_t error() { return native_array_t(nullptr, -1, nullptr); }
    static inline native_array_t empty() { return native_array_t(nullptr, 0, nullptr); }

    static inline native_array_t alloc(size_t length) {
        auto buffer = (char*)malloc(length);
        if (!buffer) return error();
        return native_array_t(buffer, length, native_array_free);
    }

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


struct mutex_lock {
    mutex_lock(std::mutex* mutex) :m(mutex) { m->lock(); }
    ~mutex_lock() { m->unlock(); }
private:
    std::mutex* m;
};

template<typename TItem>
using Consumer = void (__stdcall*)(TItem toBeConsumed);

template<typename TCollection, typename TItem>
void consume_iterable(TCollection &&collection, Consumer<TItem> consumer) {
    for (auto it = collection.begin(); it != collection.end(); ++it)
        consumer(*it);
}
