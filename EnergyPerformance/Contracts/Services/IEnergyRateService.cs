namespace EnergyPerformance.Contracts.Services;
public interface IEnergyRateService
{
    public Task<double> GetEnergyRate(string countryName, string ukRegion);
}
