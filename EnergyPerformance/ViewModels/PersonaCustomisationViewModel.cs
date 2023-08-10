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
    private ObservableCollection<ApplicationObject> applicationList = new();
    
    [ObservableProperty]
    private ObservableCollection<String> applications = new();

    private readonly PersonaModel _personaModel;

    public PersonaCustomisationViewModel(PersonaModel personaModel)
    {
        _personaModel = personaModel;

        foreach (var (path, rating) in _personaModel.ReadPersonaAndRating())
        {
            var appName = Path.GetFileNameWithoutExtension(path);
            applications.Add(appName);
            applicationList.Add(new ApplicationObject(appName, (int)Math.Round(rating)));
        }
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