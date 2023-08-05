using EnergyPerformance.Core.Helpers;

namespace EnergyPerformance.Contracts.Services;
public interface IDatabaseService
{
    public Task InitializeDB();

    public Task SaveEnergyData(EnergyUsageData data);

    public Task<EnergyUsageData> LoadUsageData();
}
