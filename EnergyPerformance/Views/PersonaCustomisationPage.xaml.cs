using System.Data;
using CommunityToolkit.WinUI.UI;
using EnergyPerformance.Models;
using EnergyPerformance.Services;
using EnergyPerformance.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using Windows.Media.AppRecording;

namespace EnergyPerformance.Views;

public sealed partial class PersonaCustomisationPage : Page
{
    public PersonaCustomisationViewModel ViewModel
    {
        get;
    }
    
    public PersonaCustomisationPage()
    {
        ViewModel = App.GetService<PersonaCustomisationViewModel>();
        InitializeComponent();
    }

    // Function that is called when item in list view is selected
    // Navigates to the Persona Slider Page and passes the selected index as a parameter
    private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        Frame.Navigate(typeof(PersonaSliderPage), PersonaList.SelectedIndex);    
    }

    // Function that is called when the Add Persona button is clicked
    // Navigates to the Persona Slider Page
    // Note - No parameter is passed, as we want to add a new persona
    private void NavigateToSliderPage (object sender, RoutedEventArgs e)
    {
        Frame.Navigate(typeof(PersonaSliderPage));
    }
}