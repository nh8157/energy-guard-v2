#pragma once
#include "libraries/ADL/ADLXHelper/Windows/Cpp/ADLXHelper.h"

using namespace adlx;

namespace gpu
{
    enum GpuVendor
    {
        GpuVendor_Amd,
        GpuVendor_Nvidia,
        GpuVendor_Unknown
    };

    GpuVendor get_primary_gpu_vendor();
    
    enum GpuType
    {
        GpuType_Discrete,
        GpuType_Integrated,
        GpuType_Unknown
    };

    struct GpuInstance
    {
        // True if the GPU allows core clock rate control.
        bool isSupported;
        int minCoreClock;
        int defaultClockRate;
        int maxCoreClock;
        GpuType type;
        const char* vendorId;
        const char* name;

        bool setCoreClock(int coreClock);
    };

}

