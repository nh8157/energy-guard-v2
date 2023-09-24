using System;
using CLI;

namespace EnergyPerformance.Elevated.Controllers
{
    public class CpuController
    {
        private ManagedController _controller;
        
        public CpuController()
        {
            _controller = new ManagedController();
        }
        
        public void MoveAllAppsToEfficiencyCores()
        {
            _controller.MoveAllAppsToEfficiencyCores();
        }
        
        public void MoveAllAppsToSomeEfficiencyCores()
        {
            _controller.MoveAllAppsToSomeEfficiencyCores();
        }
        
        public bool MoveAppToHybridCores(string target, int eCores, int pCores) 
        {
            return _controller.MoveAppToHybridCores(target, eCores, pCores);
        }
        
        public void MoveAllAppsToHybridCores(int eCores, int pCores)
        {
            _controller.MoveAllAppsToHybridCores(eCores, pCores);
        }
        
        public void ResetToDefaultCores()
        {
            _controller.ResetToDefaultCores();
        }
        
        public void DetectCoreCount()
        {
            _controller.DetectCoreCount();
        }
        
        public int TotalCoreCount()
        {
            return _controller.TotalCoreCount();
        }
        
        public int EfficiencyCoreCount()
        {
            return _controller.EfficiencyCoreCount();
        }
        
        public int PerformanceCoreCount()
        {
            return _controller.PerformanceCoreCount();
        }
    }
}