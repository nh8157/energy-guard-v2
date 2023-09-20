#pragma once
#include <string>

#include "NativeController.h"

namespace CLI
{
    public ref class ManagedController
    {
    private:
        Core::NativeController* m_NativeController;
    public:
        ManagedController();
        ~ManagedController();
        !ManagedController();
        void MoveAllAppsToEfficiencyCores();
        void MoveAllAppsToSomeEfficiencyCores();
        bool MoveAppToHybridCores(System::String^ target, int eCores, int pCores);
        void MoveAllAppsToHybridCores(int eCores, int pCores);
        void ResetToDefaultCores();
        void DetectCoreCount();
        int TotalCoreCount();
        int EfficiencyCoreCount();
        int PerformanceCoreCount();
    };

}
