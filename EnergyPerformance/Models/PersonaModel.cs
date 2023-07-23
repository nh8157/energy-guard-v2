using EnergyPerformance.Core.Contracts.Services;
using EnergyPerformance.Helpers;
using EnergyPerformance.Services;
using System.Diagnostics;

namespace EnergyPerformance.Models;
public class PersonaModel
{
    private readonly PersonaFileService _personaFileService;
    private readonly CpuInfo _cpuInfo;

    /// <summary>
    /// This field stores all existing personas in a list
    /// </summary>
    private List<PersonaEntry> _allPersonas;
    private int _nextPersonaId = 0;

    /// <summary>
    /// This property indicates whether any persona is enabled right now
    /// </summary>
    public bool IsEnabled
    {
        set; get;
    }

    /// <summary>
    /// This property points to the currently enabled persona
    /// </summary>
    public PersonaEntry? PersonaEnabled
    {
        set; get;
    }

    public PersonaModel(CpuInfo cpuInfo, PersonaFileService personaFileService)
    {
        _personaFileService = personaFileService;
        _allPersonas = new List<PersonaEntry>();
        _cpuInfo = cpuInfo;
        IsEnabled = false;
        PersonaEnabled = null;
        foreach (var persona in _allPersonas)
            if (persona.Id > _nextPersonaId)
                _nextPersonaId = persona.Id + 1;
    }

    /// <summary>
    /// This method will be executed in ActivationService, and will read persona data from persistent storage
    /// </summary>
    /// <returns></returns>
    public async Task InitializeAsync()
    {
        _allPersonas = await _personaFileService.ReadFileAsync();
    }

    /// <summary>
    /// This methods creates a new persona file based on the path and energy rating the user sets
    /// It converts the energy rating into respective CPU and GPU settings based on the hardware spec
    /// It assigns every persona a unique ID (this can be swapped with the unique ID returned by the database)
    /// </summary>
    /// <param name="path">The path to the executable of the program</param>
    /// <param name="energyRating">The position of the slider defined by the user</param>
    public async Task CreatePersona(string path, float energyRating)
    {
        _allPersonas.Add(new PersonaEntry(_nextPersonaId, path, ConvertRatingToCpuSetting(energyRating), ConvertRatingToGpuSetting(energyRating)));
        await _personaFileService.SaveFileAsync();
        _nextPersonaId += 1;
    }

    /// <summary>
    /// This method reads all existing personas
    /// And translates the raw CPU, GPU settings into the positioning of the slider
    /// </summary>
    /// <returns>A path name, energy rating pair for every application</returns>
    public List<(int, string, float)> ReadPersonaAndRating()
    {
        List<(int, string, float)> profiles = _allPersonas.Select(persona => (persona.Id, Path.GetFileName(persona.Path), 
            ConvertSettingsToRating(persona.CpuSetting, persona.GpuSetting))).ToList();
        return profiles;
    }

    /// <summary>
    /// This method is invoked when the user updates the setting on the frontend
    /// </summary>
    /// <param name="personaId">The unique ID of the persona</param>
    /// <param name="path">The path to the executable of the program</param>
    /// <param name="energyRating">User-defined slider position</param>
    public async Task UpdatePersona(int personaId, string path, float energyRating)
    {
        _allPersonas.ForEach(persona =>
        {
            if (persona.Id == personaId)
            {
                persona.Path = path;
                persona.CpuSetting = ConvertRatingToCpuSetting(energyRating);
                persona.GpuSetting = ConvertRatingToGpuSetting(energyRating);
            }
        });
        await _personaFileService.SaveFileAsync();
    }

    /// <summary>
    /// Removes the persona from the volatile and persistent memory
    /// </summary>
    /// <param name="personaId">The unique ID of the persona</param>
    public async Task DeletePersona(int personaId)
    {
        var res = _allPersonas.RemoveAll(persona => persona.Id == personaId) > 0 ? true : false;
        await _personaFileService.SaveFileAsync();
    }

    /// <summary>
    /// Invoked when a program with a program that has a defined persona in EnergyGuard is launched
    /// Applies the CPU/GPU settings that correspond to the process to the hardware
    /// </summary>
    /// <param name="personaId">The unique ID of the persona</param>
    /// <returns>Whether the enabling was successful</returns>
    public bool EnablePersona(int personaId)
    {
        var index = _allPersonas.FindIndex(persona => persona.Id == personaId);
        if (!IsEnabled && index != -1)
        {
            var persona = _allPersonas[index];
            // we need to research how to verify if a process is currently running
            // if it is not running, then the following lines should not be executed
            _cpuInfo.EnableCpuSetting(persona.Path, persona.CpuSetting);
            EnableGpuSetting(persona.GpuSetting);
            PersonaEnabled = _allPersonas.FirstOrDefault(persona => persona.Id == personaId);
            IsEnabled = true;
            return true;
        }
        return false;
    }

    /// <summary>
    /// Invoked when the user exits the selected program
    /// Disable the engaged persona setting
    /// </summary>
    /// <param name="personaId"></param>
    /// <returns></returns>
    public bool DisablePersona(int personaId)
    {
        var index = _allPersonas.FindIndex(persona => persona.Id == personaId);
        if (IsEnabled && index != -1)
        {
            var persona = _allPersonas[index];
            _cpuInfo.DisableCpuSetting(persona.Path, persona.CpuSetting);
            DisableGpuSetting(persona.GpuSetting);
            IsEnabled = false;
            PersonaEnabled = null;
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
        return (float)0.0;
    }
}
