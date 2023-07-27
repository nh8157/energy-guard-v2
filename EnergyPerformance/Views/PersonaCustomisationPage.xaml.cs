using System.Data;
using CommunityToolkit.WinUI.UI;
using EnergyPerformance.Models;
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
    const int DEFAULT = 2;

    public PersonaCustomisationViewModel ViewModel
    {
        get;
    }
    
    public PersonaCustomisationPage()
    {
        ViewModel = App.GetService<PersonaCustomisationViewModel>();
        DataContext = ViewModel;
        InitializeComponent();
        //PersonaSlider.LayoutUpdated += PersonaSlider_LayoutUpdated;
    }

    private void ShowPopup(object sender, RoutedEventArgs e)
    {
        if (!PersonaPopup.IsOpen) { PersonaPopup.IsOpen = true; }
    }

    private void HidePopup(object sender, RoutedEventArgs e)
    {
        PersonaList.DeselectAll();
        if (PersonaPopup.IsOpen) { PersonaPopup.IsOpen = false; }
    }

    private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        ShowPopup(sender, e);
        var selectedIndex = PersonaList.SelectedIndex;

        AppSelection.SelectedIndex = selectedIndex;
        if (selectedIndex != -1)
        {
            PersonaSlider.Value = ViewModel.ApplicationList[selectedIndex].Value;
        }        
    }

    private void RestoreDefault(object sender, RoutedEventArgs e)
    {
        PersonaSlider.Value = DEFAULT;
    }

    private void ApplyPersona(object sender, RoutedEventArgs e)
    {
        var selectedIndex = AppSelection.SelectedIndex;
        if (selectedIndex != -1)
        {
            var item = ViewModel.ApplicationList[selectedIndex];
            item.Value = (int)PersonaSlider.Value;
            item.EnergyRating = item.UpdateEnergyRating((int)PersonaSlider.Value);
        }
    }

    //private void PersonaSlider_LayoutUpdated(object? sender, object e)
    //{
    //    if (VisualTreeHelper.GetOpenPopupsForXamlRoot(PersonaSlider.XamlRoot).LastOrDefault() is Popup popup)
    //    {
    //        popup.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
    //    }
    //}
}