#pragma once

namespace Core
{
    class Controller
    {
    public:
        const char* m_Name;
    public:
        Controller();
        void MoveAllAppsToEfficiencyCores();
        void MoveAllAppsToSomeEfficiencyCores();
        void MoveAllAppsToHybridCores(int eCores, int pCores);
        void ResetToDefaultCores();
        void DetectCoreCount();
        int TotalCoreCount();
        int EfficiencyCoreCount();
        int PerformanceCoreCount();
    };
}