using System;
using Windows.Devices.Geolocation;

namespace EnergyPerformance.Helpers;

public class LocationInfo
{
    public virtual string Country
    {
        get;
        set;
    }
    public virtual string Postcode
    {
        get;
        set;
    }

    public LocationInfo()
    {
        Country = "Unknown";
        Postcode = "Unknown";
    }
}