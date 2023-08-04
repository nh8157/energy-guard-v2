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

    public string Region
    {
        get;
        set;
    }

    public LocationInfo()
    {
        Country = "Unknown";
        Postcode = "Unknown";
        Region = "Unknown";
    }
}
