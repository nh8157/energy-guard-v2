using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnergyPerformance.Helpers;
public class LocationInfo
{
    public string Country
    {
        get; set;
    }
    public string PostCode
    {
        get; set;
    }

    public string Region
    {
        get; set;
    }

    public LocationInfo(string country, string postCode, string region)
    {
        // UCL's address is set as the default address
        Country = country;
        PostCode = postCode;
        Region = region;
    }
    public LocationInfo()
    {
        Country = "Unknown";
        PostCode = "Unknown";
        Region = "Unknown";
    }
}
