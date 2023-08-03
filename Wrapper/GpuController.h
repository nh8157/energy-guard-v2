#pragma once
#include "ManagedObject.h"
#include "../Core/GpuAmdController.h"

namespace CLI
{
    public ref class GpuController : public ManagedObject<Core::GpuAmdController>
    {
    public:
        GpuController();
        bool IsSupported(); // returns whether the GPU supports clock rate adjustments
        bool EnableGpuSetting(int clockRate);
        bool DisableGpuSetting();
        ~GpuController();
    };

}

