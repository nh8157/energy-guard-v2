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

public sealed partial class PersonaListPage : Page
{
    public PersonaViewModel ViewModel
    {
        get;
    }
    
    public PersonaListPage()
    {
        ViewModel = App.GetService<PersonaViewModel>();
        InitializeComponent();
    }

    // Function that is called when item in list view is selected
    // Navigates to the Customise Persona Page and passes the selected index as a parameter
    private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        Frame.Navigate(typeof(CustomisePersonaPage), PersonaList.SelectedIndex);    
    }

    private void NavigateToAddPage(object sender, RoutedEventArgs e)
    {
        Frame.Navigate(typeof(AddPersonaPage));
    }

    private void DeleteButtonClicked(object sender, RoutedEventArgs e)
    {
        // Functionality to Add
    }

    private void PlayButtonClicked(object sender, RoutedEventArgs e)
    {
        // Functionality to Add
    }

    private void PauseButtonClicked(object sender, RoutedEventArgs e)
    {
        // Functionality to Add
    }
}