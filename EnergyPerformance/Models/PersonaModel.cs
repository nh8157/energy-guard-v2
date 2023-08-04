using EnergyPerformance.Helpers;
using EnergyPerformance.Services;
using System.Diagnostics;

namespace EnergyPerformance.Models;
public class PersonaModel
{
    private readonly PersonaFileService _personaFileService;
    private readonly ProcessMonitorService _processMonitorService;
    private readonly CpuInfo _cpuInfo;

    /// <summary>
    /// This field stores all existing personas in a list
    /// </summary>
    private List<PersonaEntry> _allPersonas;
    private int _nextPersonaId = 0;

    /// <summary>
    /// This property indicates whether any Persona is enabled right now
    /// </summary>
    public bool IsEnabled
    {
        set; get;
    }

    /// <summary>
    /// This property points to the currently enabled Persona
    /// </summary>
    public PersonaEntry? PersonaEnabled
    {
        set; get;
    }

    public PersonaModel(CpuInfo cpuInfo, PersonaFileService personaFileService)
    {
        _personaFileService = personaFileService;
        _allPersonas = new List<PersonaEntry>();
        _processMonitorService = new ProcessMonitorService();
        _cpuInfo = cpuInfo;
        IsEnabled = false;
        PersonaEnabled = null;

        _processMonitorService.CreationEventHandler += CreationEventHandler;
        _processMonitorService.DeletionEventHandler += DeletionEventHandler;
    }

    /// <summary>
    /// This method will be executed in ActivationService, and will read Persona data from persistent storage
    /// </summary>
    /// <returns></returns>
    public async Task InitializeAsync()
    {
        _allPersonas = await _personaFileService.ReadFileAsync();

        foreach (var persona in _allPersonas)
        {
            // Add watchers for an existing persona
            _processMonitorService.AddWatcher(persona.Path);

            // Compute the next available ID for a persona
            if (persona.Id >= _nextPersonaId)
            {
                _nextPersonaId = persona.Id + 1;
            }
        }
    }

    /// <summary>
    /// The handler is invoked when an application of interest is launched
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public void CreationEventHandler(object? sender, EventArgs e) 
    {
        var executablePath = _processMonitorService.CreatedProcess;
        App.MainWindow.DispatcherQueue.TryEnqueue(() =>
        {
            if (executablePath != null)
            {
                PersonaNotificationService.EnablePersona(executablePath);
            }
        });
    }

    /// <summary>
    /// The handler is invoked if an application of interest is closed
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public void DeletionEventHandler(object? sender, EventArgs e)
    {
        var personaName = _processMonitorService.DeletedProcess;
        if (IsEnabled && PersonaEnabled != null && personaName != null)
        {
            DisableEnabledPersona();
            App.MainWindow.DispatcherQueue.TryEnqueue(() =>
            {
                if (personaName != null)
                {
                    PersonaNotificationService.DisabledPersona(personaName);
                }
            });
        }
        else
        {
            Debug.WriteLine($"No persona enabled");
        }
    }

    /// <summary>
    /// This methods creates a new Persona file based on the path and energy rating the user sets
    /// It converts the energy rating into respective CPU and GPU settings based on the hardware spec
    /// It assigns every Persona a unique ID (this can be swapped with the unique ID returned by the database)
    /// </summary>
    /// <param name="personaName">The name of the Persona</param>
    /// <param name="energyRating">The position of the slider defined by the user</param>
    public async Task CreatePersona(string personaName, float energyRating)
    {
        if (!_allPersonas.Any(persona => persona.Path.Equals(personaName, StringComparison.OrdinalIgnoreCase)))
        {
            var entry = new PersonaEntry(_nextPersonaId, personaName, ConvertRatingToCpuSetting(energyRating), ConvertRatingToGpuSetting(energyRating));

            _allPersonas.Add(entry);

            // Add creation and deletion watchers for the application
            _processMonitorService.AddWatcher(personaName);

            await _personaFileService.SaveFileAsync();
            _nextPersonaId += 1;
        }
    }

    /// <summary>
    /// This method reads all existing Personas
    /// And translates the raw CPU, GPU settings into the positioning of the slider
    /// </summary>
    /// <returns>List of executable path name and energy rating pair for every application</returns>
    public List<(string, float)> ReadPersonaAndRating()
    {
        List<(string, float)> profiles = _allPersonas.Select(persona => (persona.Path, ConvertSettingsToRating(persona.CpuSetting, persona.GpuSetting))).ToList();
        return profiles;
    }

    /// <summary>
    /// This method is invoked when the user updates the setting on the frontend
    /// </summary>
    /// <param name="personaName">The name of the Persona</param>
    /// <param name="energyRating">User-defined slider position</param>
    public async Task UpdatePersona(string personaName, float energyRating)
    {
        _allPersonas.ForEach(persona =>
        {
            if (string.Equals(persona.Path, personaName))
            {
                persona.CpuSetting = ConvertRatingToCpuSetting(energyRating);
                persona.GpuSetting = ConvertRatingToGpuSetting(energyRating);

                _processMonitorService.RemoveWatcher(personaName);
                _processMonitorService.AddWatcher(personaName);
            }
        });
        await _personaFileService.SaveFileAsync();
    }

    /// <summary>
    /// Removes the Persona from the volatile and persistent memory
    /// </summary>
    /// <param name="personaName">The name of the Persona</param>
    public async Task DeletePersona(string personaName)
    {
        _processMonitorService.RemoveWatcher(personaName);
        var res = _allPersonas.RemoveAll(persona => persona.Path.Equals(personaName, StringComparison.OrdinalIgnoreCase)) > 0;
        await _personaFileService.SaveFileAsync();
    }

    /// <summary>
    /// Applies the CPU/GPU settings that correspond to the process to the hardware
    /// </summary>
    /// <param name="personaName">The name of the Persona</param>
    /// <returns>Whether the enabling was successful</returns>
    public bool EnablePersona(string personaName)
    {
        var index = _allPersonas.FindIndex(persona => persona.Path.Equals(personaName, StringComparison.OrdinalIgnoreCase));

        if (index != -1)
        {
            var persona = _allPersonas[index];
            // we need to research how to verify if a process is currently running
            // if it is not running, then the following lines should not be executed
            PersonaEnabled = persona;
            IsEnabled = true;

            // pass the settings to CPU and GPU
            _cpuInfo.EnableCpuSetting(persona.Path, persona.CpuSetting);
            EnableGpuSetting(persona.GpuSetting);

            Debug.WriteLine($"Persona for {personaName} enabled");

            return true;
        }
        Debug.WriteLine($"Cannot find persona for {personaName}");
        return false;
    }

    /// <summary>
    /// Invoked when the user exits the selected program
    /// Disable the engaged Persona setting
    /// </summary>
    /// <param name="personaName"></param>
    /// <returns></returns>
    public bool DisableEnabledPersona()
    {
        if (PersonaEnabled != null)
        {
            var personaName = PersonaEnabled.Path;
            Debug.WriteLine($"Disabling persona for {personaName}");

            if (PersonaEnabled == null)
            {
                return false;
            }
            _cpuInfo.DisableCpuSetting(personaName, PersonaEnabled.CpuSetting);
            DisableGpuSetting(PersonaEnabled.GpuSetting);

            PersonaEnabled = null;
            IsEnabled = false;

            return true;
        }
        return false;
    }

    // we might want to move these methods into their own classes
    private void EnableGpuSetting(int gpuSetting)
    {
    }

    private void DisableGpuSetting(int gpuSetting)
    {
    }

    private int ConvertRatingToCpuSetting(float energyRating)
    {
        return 0;
    }
    private int ConvertRatingToGpuSetting(float energyRating)
    {
        return 0;
    }

    private float ConvertSettingsToRating(int cpuSetting, int gpuSetting)
    {
        return 0.0f;
    }
}
