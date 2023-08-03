using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using EnergyPerformance.Helpers;
using EnergyPerformance.Models;

namespace EnergyPerformance.ViewModels;
public partial class PersonaViewModel : ObservableRecipient
{
    private readonly PersonaModel _model;

    public List<(string, float)> PersonasAndRatings;

    public PersonaEntry PersonaEnabled
    {
        get => _model.PersonaEnabled;
        set
        {
            _model.PersonaEnabled = value;
            OnPropertyChanged(nameof(PersonaEnabled));
        }
    }

    public PersonaViewModel(PersonaModel model)
    {
        _model = model; 
        InitializePersonaModel();
    }

    private void InitializePersonaModel()
    {
        PersonasAndRatings = _model.ReadPersonaAndRating();
    }
}
