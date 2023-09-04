using System.CodeDom;
using System.Diagnostics;
using EnergyPerformance.Core.Contracts.Services;
using EnergyPerformance.Helpers;

namespace EnergyPerformance.Services;
public class PersonaFileService
{
    private const string _defaultApplicationDataFolder = "EnergyPerformance/ApplicationData";
    private const string _personaFile = "Persona.json";

    private readonly IFileService _fileService;

    private readonly string _localApplicationData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
    private readonly string _applicationDataFolder;

    private List<PersonaEntry> _personaData;

    private bool _isInitialized;

    public PersonaFileService(IFileService fileService)
    {
        _fileService = fileService;
        _applicationDataFolder = Path.Combine(_localApplicationData, _defaultApplicationDataFolder);

        _personaData = new List<PersonaEntry>();
    }

    private async Task InitializeAsync()
    {
        if (!_isInitialized)
        {
            var data = await Task.Run(() => _fileService.Read<List<PersonaEntry>>(_applicationDataFolder, _personaFile));
            if (data != null)
                _personaData = data;
            Debug.WriteLine("Reading personas from disk");
            _isInitialized = true;
        }
    }

    public async Task SaveFileAsync()
    {
        await Task.Run(() => _fileService.Save(_applicationDataFolder, _personaFile, _personaData));
    }

    public async virtual Task<List<PersonaEntry>> ReadFileAsync()
    {
        if (!_isInitialized)
        {
            await InitializeAsync();
        }
        return _personaData;
    }
}
