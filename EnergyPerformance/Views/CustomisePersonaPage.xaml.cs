using System.Data;
using System.Runtime.CompilerServices;
using ABI.Windows.ApplicationModel.Activation;
using CommunityToolkit.WinUI.UI;
using EnergyPerformance.Models;
using EnergyPerformance.Services;
using EnergyPerformance.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Windows.Media.AppRecording;

namespace EnergyPerformance.Views;

public sealed partial class CustomisePersonaPage : Page
{
    const float DEFAULT = 2.0f;

    public PersonaViewModel ViewModel
    {
        get;
    }
    
    public CustomisePersonaPage()
    {
        ViewModel = App.GetService<PersonaViewModel>();
        InitializeComponent();
    }

    // Function to apply persona to the selected application
    // Grabs the index of the application in the list
    // Sets the corresponding energy value and image path accordingly in the view model
    // Navigates to the Persona List Page
    private void ApplyPersona(object sender, RoutedEventArgs e)
    {
        var selectedIndex = AppSelection.SelectedIndex;
        if (selectedIndex != -1)
        {
            var item = ViewModel.PersonasAndRatings[selectedIndex];
            ViewModel.Update(selectedIndex, (float)PersonaSlider.Value);
            item.EnergyValue = (float)PersonaSlider.Value;
            item.EnergyRating = item.UpdateEnergyRating((int)PersonaSlider.Value);
            Frame.Navigate(typeof(PersonaListPage));
        }
    }

    // Function called when Go Back button is clicked
    // Navigates to the Persona List Page
    private void NavigateToListPage(object sender, RoutedEventArgs e)
    {
        Frame.Navigate(typeof(PersonaListPage));
    }

    // Function called when the selection in the combo box is changed
    // Gets the selected index from the combo box
    // Grabs the corresponding energy value, and sets the slider value to it
    private void UpdateSliderValue(object sender, RoutedEventArgs e)
    {
        var selectedIndex = AppSelection.SelectedIndex;
        if (selectedIndex != -1)
        {
            PersonaSlider.Value = ViewModel.PersonasAndRatings[selectedIndex].EnergyValue;
        }
    }

    // Overriden OnNavigatedTo - For when a parameter is passed
    // Updates the form values accordingly, if parameter is passed
    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        if (e.Parameter is int && !e.Equals(-1))
        {
            var index = (int)e.Parameter;
            AppSelection.SelectedIndex = index;

            PersonaSlider.Value = ViewModel.PersonasAndRatings[index].EnergyValue;  
        }
        base.OnNavigatedTo(e);
    }
}