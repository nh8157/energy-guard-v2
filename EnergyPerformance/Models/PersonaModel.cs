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

    public bool RemovePersona(int personaId)
    {
        return _allPersonas.RemoveAll(persona => persona.Id == personaId) > 0 ? true : false;
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
