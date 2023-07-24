using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Security.Cryptography.X509Certificates;
using CommunityToolkit.Mvvm.ComponentModel;
using EnergyPerformance.Models;

namespace EnergyPerformance.ViewModels;

public class PersonaCustomisationViewModel : ObservableRecipient
{
    public readonly ObservableCollection<String> Applications = new();

    public PersonaCustomisationViewModel()
    {
        Applications.Add("Steam");
        Applications.Add("Spotify");
        Applications.Add("Word");
        Applications.Add("Minecraft");
        Applications.Add("Chrome");
    }
}