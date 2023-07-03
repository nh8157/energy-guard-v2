using EnergyPerformance.Core.Contracts.Services;
using EnergyPerformance.Core.Helpers;


namespace EnergyPerformance.Services;


/// <summary>
/// Class that saves and reads energy usage data to/from LocalAppData storage.
/// </summary>
public class EnergyUsageFileService
{
    private const string _defaultApplicationDataFolder = "EnergyPerformance/ApplicationData";
    private const string _energyUsageFile = "Data.json";

    private readonly IFileService _fileService;

    private readonly string _localApplicationData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
    private readonly string _applicationDataFolder;


    private EnergyUsageData _energyUsage;

    public EnergyUsageData EnergyUsage => _energyUsage;

    private bool _isInitialized;


    /// <summary>
    /// Initializes a new instance of the <see cref="EnergyUsageFileService"/> class.
    /// </summary>
    public EnergyUsageFileService(IFileService fileService)
    {
        _fileService = fileService;
        // get file path to Local AppData folder in Windows
        _applicationDataFolder = Path.Combine(_localApplicationData, _defaultApplicationDataFolder);
        
        _energyUsage = new EnergyUsageData();
    }


    /// <summary>
    /// Performs async operations required for the EnergyUsageFileService at startup.
    /// Called by the ActivationService at launch.
    /// </summary>
    private async Task InitializeAsync()
    {
        if (!_isInitialized)
        {
            // Opens the file saving energy usage data, if any
            // or creates a new file
            _energyUsage = await Task.Run(() => _fileService.Read<EnergyUsageData>(_applicationDataFolder, _energyUsageFile)) ?? new EnergyUsageData();
            Console.WriteLine(_localApplicationData);
            _isInitialized = true;
        }
    }

    /// <summary>
    /// Saves the current state of <see cref="EnergyUsageData"/> to LocalAppData storage.
    /// </summary>
    public async Task SaveFileAsync()
    {
        await Task.Run(() => _fileService.Save(_applicationDataFolder, _energyUsageFile, _energyUsage));
    }


    /// <summary>
    /// Reads energy usage data stored in LocalAppData storage.
    /// </summary>
    public async Task<EnergyUsageData> ReadFileAsync()
    {
        if (!_isInitialized) {
            await InitializeAsync();
        }
        return _energyUsage;
    }

}
