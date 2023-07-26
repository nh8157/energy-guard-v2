using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Security.Cryptography.X509Certificates;
using CommunityToolkit.Mvvm.ComponentModel;
using EnergyPerformance.Models;

namespace EnergyPerformance.ViewModels;

public class PersonaCustomisationViewModel : ObservableRecipient
{

    public readonly ObservableCollection<String> Applications = new();
    public readonly ObservableCollection<ApplicationObject> ApplicationList = new();

    public PersonaCustomisationViewModel()
    {
        Applications.Add("Steam");
        Applications.Add("Spotify");
        Applications.Add("Word");
        Applications.Add("Minecraft");
        Applications.Add("Chrome");

        ApplicationList.Add(new ApplicationObject("Steam", 3));
        ApplicationList.Add(new ApplicationObject("Spotify", 1));
        ApplicationList.Add(new ApplicationObject("Word", 1));
        ApplicationList.Add(new ApplicationObject("Minecraft", 3));
        ApplicationList.Add(new ApplicationObject("Chrome", 2));
    }
}

public class ApplicationObject
{
    public string AppName;
    public string EnergyRating;


    private int MIN = 1;
    private int MAX = 3;

    public ApplicationObject(string appName, int energyRating)
    {
        AppName = appName;
        EnergyRating = Reflection(energyRating);
    }

    string Reflection(int energyRating)
    {
        var append = MAX - energyRating + MIN;
        var path = "ms-appx:///Assets/Leaf" + append.ToString() + ".png";
        return path;
    }
}