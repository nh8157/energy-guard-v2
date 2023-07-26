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

public partial class PersonaCustomisationViewModel : ObservableRecipient, INotifyPropertyChanged
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

        applicationList.Add(new ApplicationObject("Steam", 3));
        applicationList.Add(new ApplicationObject("Spotify", 1));
        applicationList.Add(new ApplicationObject("Word", 1));
        applicationList.Add(new ApplicationObject("Minecraft", 3));
        applicationList.Add(new ApplicationObject("Chrome", 2));
    }
}

public partial class ApplicationObject : ObservableRecipient, INotifyPropertyChanged
{
    [ObservableProperty]
    private string appName;

    [ObservableProperty]
    private int value;

    [ObservableProperty]
    private string energyRating;

    private readonly int MIN = 1;
    private readonly int MAX = 3;

    public event PropertyChangedEventHandler PropertyChanged;

    public ApplicationObject(string _appName, int _energyRating)
    {
        appName = _appName;
        value = _energyRating;
        energyRating = UpdateEnergyRating(_energyRating);
    }

    public string UpdateEnergyRating(int _value)
    {
        var append = MAX - _value + MIN;
        var path = "ms-appx:///Assets/Leaf" + append.ToString() + ".png";
        return path;
    }

    public string EnergyRatingPath
    {
        get => energyRating;

        set
        {
            if (energyRating != value)
            {
                energyRating = value;
                OnPropertyChanged();
            }
        }
    }

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = "")
    {
        if (PropertyChanged != null)
        {
            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}