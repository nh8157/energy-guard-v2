#include "GpuController.h"
#include "ManagedObject.h"


namespace CLI
{
    GpuController::GpuController()
        : ManagedObject(new Core::GpuAmdController())
    {
        System::Diagnostics::Debug::WriteLine("Creating a new Entity-wrapper object for GPU.");
    }

    GpuController::~GpuController()
    {
        m_Instance->~GpuController();
    }

    bool GpuController::IsSupported()
    {
        m_Instance->IsSupported();
    }

    bool GpuController::DisableGpuSetting()
    {
        m_Instance->DisableGpuSetting();
    }

    bool GpuController::EnableGpuSetting(int clockRate)
    {
        m_Instance->EnableGpuSetting(clockRate);
    }



}
