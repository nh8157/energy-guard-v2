#include "ManagedController.h"

#include <string>
#include <msclr/marshal_cppstd.h>

using namespace CLI;

ManagedController::ManagedController()
{
    this->m_NativeController = new Core::NativeController();
}

ManagedController::~ManagedController()
{
    delete this->m_NativeController;
}

ManagedController::!ManagedController()
{
    delete this->m_NativeController;
}

void ManagedController::DetectCoreCount()
{
    m_NativeController->DetectCoreCount();
}

int ManagedController::TotalCoreCount()
{
    return m_NativeController->TotalCoreCount();
}

int ManagedController::EfficiencyCoreCount()
{
    return m_NativeController->EfficiencyCoreCount();
}

int ManagedController::PerformanceCoreCount()
{
    return m_NativeController->PerformanceCoreCount();
}

void ManagedController::MoveAllAppsToEfficiencyCores()
{
    m_NativeController->MoveAllAppsToEfficiencyCores();
}

void ManagedController::MoveAllAppsToSomeEfficiencyCores()
{
    m_NativeController->MoveAllAppsToSomeEfficiencyCores();
}

bool ManagedController::MoveAppToHybridCores(System::String^ target, int eCores, int pCores)
{
    std::string targetString = msclr::interop::marshal_as<std::string>(target);
    const wchar_t* targetWChar = reinterpret_cast<const wchar_t*>(targetString.c_str());
    return m_NativeController->MoveAppToHybridCores(targetWChar, eCores, pCores);
}

void ManagedController::MoveAllAppsToHybridCores(int eCores, int pCores)
{
    m_NativeController->MoveAllAppsToHybridCores(eCores, pCores);
}

void ManagedController::ResetToDefaultCores()
{
    m_NativeController->ResetToDefaultCores();
}
