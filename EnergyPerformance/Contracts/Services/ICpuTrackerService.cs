using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnergyPerformance.Contracts.Services;
public interface ICpuTrackerService
{
    public bool SupportedCpu
    {
        get;
    }

    public int CurrentMode
    {
        get;
    }

}
