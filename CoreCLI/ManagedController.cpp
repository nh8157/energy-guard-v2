#include "ManagedController.h"

#include <iostream>
#include <ostream>
#include <string>
#include <msclr\marshal.h>
#include <msclr\marshal_cppstd.h>

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
    std::wstring str = msclr::interop::marshal_as<std::wstring>(target);
    const wchar_t* wstr = str.c_str();
    return m_NativeController->MoveAppToHybridCores(wstr, eCores, pCores);
}

void ManagedController::MoveAllAppsToHybridCores(int eCores, int pCores)
{
    m_NativeController->MoveAllAppsToHybridCores(eCores, pCores);
}

void ManagedController::ResetToDefaultCores()
{
    m_NativeController->ResetToDefaultCores();
}
