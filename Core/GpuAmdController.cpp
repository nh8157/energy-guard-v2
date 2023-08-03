#include "GpuAmdController.h"

#include "GpuAmd.h"
#include "libraries/ADL/ADLXHelper/Windows/Cpp/ADLXHelper.h"

ADLXHelper g_ADLXHelp;

namespace Core
{
    GpuAmdController::GpuAmdController()
    {
        g_ADLXHelp.Initialize();
        m_AdlxGpu = amd_gpu::get_discrete_gpu();
        m_GpuInstance = amd_gpu::get_gpu_instance(m_AdlxGpu);
    }

    
    bool GpuAmdController::IsSupported()
    {
        return m_GpuInstance.isSupported;
    }

    bool GpuAmdController::DisableGpuSetting()
    {
        // TODO: Find how to get default frequency
        return true;
    }

    bool GpuAmdController::EnableGpuSetting(int clockRate)
    {
        return amd_gpu::set_gpu_clock(m_AdlxGpu, clockRate);
    }

    GpuAmdController::~GpuAmdController()
    {
        g_ADLXHelp.Terminate();
    }
}