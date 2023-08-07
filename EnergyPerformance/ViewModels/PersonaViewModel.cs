using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using CommunityToolkit.Mvvm.ComponentModel;
using EnergyPerformance.Helpers;
using EnergyPerformance.Models;
using Microsoft.UI.Xaml.CustomAttributes;
using Windows.ApplicationModel.Activation;
using Windows.Media.Core;

namespace EnergyPerformance.ViewModels;

public partial class PersonaViewModel : ObservableRecipient
{
    private readonly PersonaModel _model;

    [ObservableProperty]
    private static ObservableCollection<PersonaObject> personasAndRatings = new();

    [ObservableProperty]
    private static ObservableCollection<string> applicationList = new();

    private static bool do_once = true;

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
        Initialise();
    }

    private void Initialise()
    {
        if (do_once)
        {
            var list = _model.ReadPersonaAndRating();
            foreach (var item in list)
            {
                Add(item.Item1, item.Item2);
            }

            AddDummyData();

            do_once = false;
        }
        
    }

    private void Add(string _appName, float _energyRating)
    {
        personasAndRatings.Add(new PersonaObject(_appName, _energyRating));
        applicationList.Add(_appName);
    }

    // For Debugging Purposes only - To be removed eventually
    private void AddDummyData()
    {
        Add("Steam", 2.0f);
        Add("Spotify", 3.0f);
        Add("Word", 2.5f);
        Add("Minecraft", 1.0f);
        Add("Chrome", 1.3f);
    }
}

// Class for Application Object - Inherits from INotifyPropertyChanged - Notifies the View that a change has occured
public partial class PersonaObject : INotifyPropertyChanged
{
    private string appName;
    private float energyValue;
    private string energyRating;

    public PersonaObject(string _appName, float _energyRating)
    {
        appName = _appName;
        energyValue = _energyRating;
        energyRating = UpdateEnergyRating((int)_energyRating);
    }

    // UpdateEnergyRating - Takes the truncated value of the energy rating
    // Allocates the image path accordignly
    public string UpdateEnergyRating(int value)
    {
        var path = "ms-appx:///Assets/Leaf" + value.ToString() + ".png";
        return path;
    }

    public string AppName
    {
        get => appName;
        set => appName = value;
    }

    public float EnergyValue
    {
        get => energyValue;
        set => energyValue = value;
    }

    // Getter and Setter for the EnergyRating property
    // When a new value is set, raise the property indicating that the value has changed and the UI needs to update accordingly
    public string EnergyRating
    {
        get => energyRating;
        set
        {
            energyRating = value;
            OnPropertyUpdated("EnergyRating");
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    private void OnPropertyUpdated(string propertyName)
    {
        if (PropertyChanged != null)
        {
            PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}