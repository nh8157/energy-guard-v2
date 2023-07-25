using System.Collections.ObjectModel;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using EnergyPerformance.Helpers;
using EnergyPerformance.Models;

namespace EnergyPerformance.ViewModels;

public class PersonaCustomisationViewModel : ObservableRecipient
{
    public readonly ObservableCollection<String> Applications = new();

    private readonly PersonaModel _model;

    public List<(int, string, float)> PersonasAndRatings;

    public (int, string, float) PersonaEnabled
    {
        get; set;
    }



    public PersonaCustomisationViewModel(PersonaModel model)
    {
        Applications.Add("Steam");
        Applications.Add("Spotify");
        Applications.Add("Word");
        Applications.Add("Minecraft");
        Applications.Add("Chrome");
        _model = model;
        PersonasAndRatings = _model.ReadPersonaAndRating();
        PersonaEnabled = (-1, "Default", 0);
    }


    public void EnablePersona(int index)
    {
        if(_model.IsEnabled && _model.PersonaEnabled != null)
        {
            PersonaEntry enabledPersona = _model.PersonaEnabled;
            _model.DisablePersona(enabledPersona.Id);
        }
        int personaID = PersonasAndRatings[index].Item1;
        if (_model.EnablePersona(personaID))
        {
            PersonaEnabled = PersonasAndRatings[personaID];
        }
    }

    public void RestorePersona()
    {
        int enabledPersonaId = PersonaEnabled.Item1;
        if(_model.DisablePersona(enabledPersonaId))
        {
            PersonaEnabled = (-1, "Default", 0);
        }
    }
}