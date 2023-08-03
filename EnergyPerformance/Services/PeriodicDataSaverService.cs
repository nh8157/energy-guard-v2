using System.Diagnostics;
using Microsoft.Extensions.Hosting;
using EnergyPerformance.Models;

namespace EnergyPerformance.Services;

/// <summary>
/// Hosted Service that saves the data in the EnergyUsageModel to a file every 5 minutes.
/// </summary>
public class PeriodicDataSaverService : BackgroundService
{
    private const int SaveInterval = 5;
    private readonly PeriodicTimer _periodicTimer = new(TimeSpan.FromMinutes(SaveInterval));
    private readonly EnergyUsageModel _model;

    public PeriodicDataSaverService(EnergyUsageFileService fileService, EnergyUsageModel model)
    {
        _model = model;
    }


    /// <summary>
    /// Executes DoAsync once every 5 minutes.
    /// </summary>
    protected async override Task ExecuteAsync(CancellationToken token)
    {
        Debug.WriteLine("PeriodicDataSaverService.ExecuteAsync");
        while (await _periodicTimer.WaitForNextTickAsync(token) && !token.IsCancellationRequested)
        {
            await DoAsync();
        }
    }

    /// <summar>
    /// Saves the data in the model to a file.
    /// </summary>
    private async Task DoAsync()
    {
        Debug.WriteLine("PeriodicDataSaverService.DoAsync");
        await _model.Save();
    }


    /// <summary>
    /// Called when the Host is shutting down.
    /// </summary>
    public async override Task StopAsync(CancellationToken cancellationToken)
    {

        Debug.WriteLine("PeriodicDataSaverService.StopAsync");
        await base.StopAsync(cancellationToken);
    }

}
