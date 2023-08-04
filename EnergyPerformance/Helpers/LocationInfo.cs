namespace EnergyPerformance.Helpers;

public class LocationInfo
{
    public string Country
    {
        get;
        set;
    }
    public string Postcode
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
