using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnergyPerformance.Temporary;

// THIS IS A TEMPORARY CLASS THAT IS ONLY USED WHEN DOCUMENTATION NEEDS TO BE GENERATED
// As the Core and Wrapper projects are in C++ and C++/CLI respectively, the documentation generator tool docfx, is unable to generate documentation for these projects.
// Due to this, these projects are not included in the documentation build process and therefore the Controller class is not included in the documentation.
// In order to allow the project to build, this temporary class is used to replace the Controller class in the CpuInfo class.


public class Controller
{
    public int PerformanceCoreCount()
    {
        return 4;
    }

    public int EfficiencyCoreCount() 
    {
        return 4;
    }

    public int TotalCoreCount()
    {
        return 12;
    }

    public void MoveAllAppsToHybridCores()
    {
        return;
    }

    public void ResetToDefaultCores()
    {
        return;
    }
}
