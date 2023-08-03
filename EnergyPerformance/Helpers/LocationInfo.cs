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
    public string Postcode
    {
        get; set;
    }
    public LocationInfo(string country, string postcode)
    {
        // UCL's address is set as the default address
        Country = country;
        Postcode = postcode;
    }
    public LocationInfo()
    {
        Country = "Unknown";
        Postcode = "Unknown";
    }
}