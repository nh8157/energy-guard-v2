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
    public LocationInfo(string country, string postCode)
    {
        // UCL's address is set as the default address
        Country = country;
        PostCode = postCode;
    }
    public LocationInfo()
    {
        Country = "Unknown";
        PostCode = "Unknown";
    }
}