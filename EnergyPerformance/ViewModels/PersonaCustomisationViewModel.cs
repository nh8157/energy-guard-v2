using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using CommunityToolkit.Mvvm.ComponentModel;
using EnergyPerformance.Models;
using Microsoft.UI.Xaml.CustomAttributes;
using Windows.ApplicationModel.Activation;
using Windows.Media.Core;

namespace EnergyPerformance.ViewModels;

public partial class PersonaCustomisationViewModel : ObservableRecipient
{
    [ObservableProperty]
    private ObservableCollection<String> applications = new();

    [ObservableProperty]
    private ObservableCollection<ApplicationObject> applicationList = new();

    public PersonaCustomisationViewModel()
    {
        applications.Add("Steam");
        applications.Add("Spotify");
        applications.Add("Word");
        applications.Add("Minecraft");
        applications.Add("Chrome");

        // Value must range between 1.0f and 3.0f
        // Any invalid values will result in no image being displayed
        applicationList.Add(new ApplicationObject("Steam", 2.1f));
        applicationList.Add(new ApplicationObject("Spotify", 1.2f));
        applicationList.Add(new ApplicationObject("Word", 1.4f));
        applicationList.Add(new ApplicationObject("Minecraft", 2.5f));
        applicationList.Add(new ApplicationObject("Chrome", 1.8f));
    }
}

// Class for Application Object - Inherits from INotifyPropertyChanged - Notifies the View that a change has occured
public partial class ApplicationObject : INotifyPropertyChanged
{
    public string appName;
    private float energyValue;
    private string energyRating;

    private readonly int MIN = 1;
    private readonly int MAX = 3;

    public ApplicationObject(string _appName, float _energyRating)
    {
        appName = _appName;
        energyValue = _energyRating;
        energyRating = UpdateEnergyRating((int)_energyRating);
    }

    public string UpdateEnergyRating(int _value)
    {
        var append = MAX - _value + MIN;
        var path = "ms-appx:///Assets/Leaf" + append.ToString() + ".png";
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