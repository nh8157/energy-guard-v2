namespace EnergyPerformance.Contracts.Services;
public interface ILocationService
{
    public string Country
    {
        get;
    }

    public string PostCode
    {
        get;
    }

    public string Region
    {
        get;
    }

}