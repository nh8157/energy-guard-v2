using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnergyPerformance.Helpers;
public class CarbonIntensityInfo
{
    private double _carbonIntensity;
    public double CarbonIntensity
    {
        get => _carbonIntensity;
        set => _carbonIntensity = value;
    }
    public CarbonIntensityInfo()
    {
        // Default carbon intensity is 100
        CarbonIntensity = 100;
    }
}
