#pragma once

namespace Core
{
    class GpuController
    {
    public:
        GpuController();
        virtual bool IsSupported(); // returns whether the GPU supports clock rate adjustments
        virtual bool EnableGpuSetting(int clockRate);
        virtual bool DisableGpuSetting();
        virtual ~GpuController() = default;
    };

}

