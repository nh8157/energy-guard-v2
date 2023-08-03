#pragma once

#include "GpuInstance.h"
#include "libraries/ADL/Include/IGPUManualGFXTuning.h"

using namespace gpu;

namespace amd_gpu {
    
    IADLXGPUPtr get_discrete_gpu();
    IADLXGPUListPtr get_all_gpus();  
    GpuInstance get_gpu_instance(IADLXGPUPtr gpu);
    bool set_gpu_states(IADLXManualGraphicsTuning1Ptr manualGFXTuning1);
    bool set_gpu_states(IADLXManualGraphicsTuning2Ptr manualGFXTuning2);
    bool set_gpu_clock(IADLXGPUPtr gpu, int newClockMHz);
}