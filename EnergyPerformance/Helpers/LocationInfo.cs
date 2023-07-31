namespace EnergyPerformance.Helpers;

public class LocationInfo
{
    public string Country
    {
        get;
        set;
    }
    public string PostCode
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
        PostCode = "Unknown";
        Region = "Unknown";
    }
}
