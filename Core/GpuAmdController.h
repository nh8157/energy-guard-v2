#pragma once
#include "GpuController.h"
#include "GpuInstance.h"

namespace Core
{
    class GpuAmdController : public GpuController
    {
    public:
        GpuAmdController();
        bool IsSupported(); // returns whether the GPU supports clock rate adjustments
        bool EnableGpuSetting(int clockRate);
        bool DisableGpuSetting();
        ~GpuAmdController();
    private:
        gpu::GpuInstance m_GpuInstance;
        IADLXGPUPtr m_AdlxGpu;
    };;
}
