namespace EnergyPerformance.Helpers;
using CoreCounter;
using System.Diagnostics;

public class Controller
{
    private int _eCoreCount;
    private int _pCoreCount;
    private int _lCoreCount;
    private int _hyperThreadCount;

    private int _affinityOffset;
    private int _tmpAffinity;

    private int _eCoreMask;
    private int _pCoreMask;
    private int _lCoreMask;
    
    private static int AffinityMaskGenerator(int coreCount)
    {
        var mask = 0;
        for (var i = 0; i < coreCount; i++)
        {
            mask |= 1 << i;
        }
        return mask;
    }

    public Controller()
    {
        // Get the number of cores on the system
        var counter = new Counter();
        _eCoreCount = counter.eCoreCount;
        _pCoreCount = counter.pCoreCount;
        _lCoreCount = counter.lCoreCount;

        // The number of hyper-threads on the system is the number of logical cores minus the number of efficiency cores
        _hyperThreadCount = _lCoreCount - _eCoreCount;

        // Generate the affinity masks for each core type
        var coreOffset = _eCoreCount + _lCoreCount;
        _affinityOffset = AffinityMaskGenerator(_lCoreCount);
        _tmpAffinity = AffinityMaskGenerator(_hyperThreadCount);

        _eCoreMask = _tmpAffinity - _affinityOffset;
        _pCoreMask = AffinityMaskGenerator(_hyperThreadCount);
        _lCoreMask = AffinityMaskGenerator(_lCoreCount);
    }
    
    /**
     * <summary>
     * Sets the affinity of a given process to a given affinity mask
     * </summary>
     * <param name="process">The process to set the affinity of</param>
     * <param name="affinityMask">The affinity mask to set the process to</param>
     */
    public void SetAffinityOfProcess(Process process, int affinityMask)
    {
        process.ProcessorAffinity = (IntPtr) affinityMask;
    }
    
    public void SetAffinityOfAllProcesses(int affinityMask)
    {
        var processes = Process.GetProcesses();
        foreach (var process in processes)
        {
            SetAffinityOfProcess(process, affinityMask);
        }
    }

    public void MoveAllAppsToEfficiencyCores()
    {
        // Ensure that the system has more than 2 efficiency cores
        if (_eCoreCount < 2)
        {
            return;
        }

        var eCoreUser = 2;
        var coreOffset = eCoreUser + _hyperThreadCount;
        
        _affinityOffset = AffinityMaskGenerator(_hyperThreadCount);
        var tmpAffinity = AffinityMaskGenerator(coreOffset);
        var eCoreAffinity = tmpAffinity - _affinityOffset;
        
        SetAffinityOfAllProcesses(eCoreAffinity);
    }

    public void MoveAllAppsToHybridCores(int eCores, int pCores)
    {
        // Check that the number of efficiency cores and performance cores is valid
        if ((eCores <= 0 && pCores <= 0) || eCores > _eCoreCount || pCores % 2 == 1 || pCores > _pCoreCount)
        {
            return;
        }
        
        var hThreadAffinity = AffinityMaskGenerator(pCores);
        var coreOffset = eCores + _hyperThreadCount;

        _affinityOffset = AffinityMaskGenerator(_hyperThreadCount);
        var tmpAffinity = AffinityMaskGenerator(coreOffset);
        var eCoreAffinity = tmpAffinity - _affinityOffset;

        int hybridAffinity;
        if (pCores > 0 && pCores % 2 == 0)
        { 
            hybridAffinity = eCoreAffinity + hThreadAffinity;
        } else { 
            hybridAffinity = eCoreAffinity; 
        }
        
        SetAffinityOfAllProcesses(hybridAffinity);
    }

    public int TotalCoreCount()
    {
        return _lCoreCount;
    }
    
    public int EfficiencyCoreCount()
    {
        return _eCoreCount;
    }
    
    public int PerformanceCoreCount()
    {
        return _pCoreCount;
    }

    public void ResetToDefaultCores()
    {
        SetAffinityOfAllProcesses(AffinityMaskGenerator(_lCoreCount));
    }
}