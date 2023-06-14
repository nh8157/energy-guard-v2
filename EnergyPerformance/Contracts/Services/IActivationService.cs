namespace EnergyPerformance.Contracts.Services;

public interface IActivationService
{
    Task ActivateAsync(object activationArgs);

    Task StartupAsync();
}
