using EnergyPerformance.Helpers;

namespace EnergyPerformance.Models;
public class PersonaModel
{
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

    public PersonaModel()
    {
        _allPersonas = new List<PersonaEntry>();
        IsEnabled = false;
        PersonaEnabled = null;
        foreach (var persona in _allPersonas)
            if (persona.Id > _nextPersonaId)
                _nextPersonaId = persona.Id + 1;
    }

    public void CreatePersona(string path, float energyRating)
    {
        _allPersonas.Add(new PersonaEntry(_nextPersonaId, path, ConvertRatingToCpuSetting(energyRating), ConvertRatingToGpuSetting(energyRating)));
        _nextPersonaId += 1;
    }

    public List<(string, float)> ReadPersonaAndRating()
    {
        List<(string, float)> profiles = _allPersonas.Select(persona => (persona.Path, ConvertSettingsToRating(persona.CpuSetting, persona.GpuSetting))).ToList();
        return profiles;
    }

    public void UpdatePersona(int personaId, string path, float energyRating)
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
    }

    public bool DeletePersona(int personaId)
    {
        return _allPersonas.RemoveAll(persona => persona.Id == personaId) > 0 ? true : false;
    }

    public bool EnablePersona(int personaId)
    {
        var index = _allPersonas.FindIndex(persona => persona.Id == personaId);
        if (!IsEnabled && index != -1)
        {
            var persona = _allPersonas[index];
            // we need to research how to verify if a process is currently running
            // if it is not running, then the following lines should not be executed
            EnableCpuSetting(persona.Path, persona.CpuSetting);
            EnableGpuSetting(persona.GpuSetting);
            return true;
        }
        return false;
    }

    public bool DisablePersona(int personaId)
    {
        var index = _allPersonas.FindIndex(persona => persona.Id == personaId);
        if (IsEnabled && index != -1)
        {
            var persona = _allPersonas[index];
            DisableCpuSetting(persona.Path, persona.CpuSetting);
            DisableGpuSetting(persona.GpuSetting);
            return true;
        }
        return false;
    }

    // we might want to move these methods into their own classes
    private void EnableCpuSetting(string path, int cpuSetting)
    {
    }

    private void DisableCpuSetting(string path, int cpuSetting)
    {
    }

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
