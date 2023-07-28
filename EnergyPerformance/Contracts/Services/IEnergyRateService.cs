namespace EnergyPerformance.Contracts.Services;
public interface IEnergyRateService
{
    public Task<double> GetEnergyRateAsync(string country, string ukRegion="");
}
