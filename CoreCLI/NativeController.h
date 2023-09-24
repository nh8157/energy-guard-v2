#pragma once

namespace Core
{
    class NativeController
    {
    public:
        NativeController();
        void MoveAllAppsToEfficiencyCores();
        void MoveAllAppsToSomeEfficiencyCores();
        bool MoveAppToHybridCores(const wchar_t* target, int eCores, int pCores);
        void MoveAllAppsToHybridCores(int eCores, int pCores);
        void ResetToDefaultCores();
        void DetectCoreCount();
        int TotalCoreCount();
        int EfficiencyCoreCount();
        int PerformanceCoreCount();

    private:
        int CreateAffinityMask(int eCores, int pCores);
    };
}