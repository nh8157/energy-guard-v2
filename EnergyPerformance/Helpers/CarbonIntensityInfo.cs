using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnergyPerformance.Helpers;
public class CarbonIntensityInfo
{
    private double _carbonIntensity;
    private string _location;
    public string Location
    {
        get => _location;
        set => _location = value;
    }
    public double CarbonIntensity
    {
        get => _carbonIntensity;
        set => _carbonIntensity = value;
    }
    public CarbonIntensityInfo()
    {
        CarbonIntensity = 100;
    }
}
