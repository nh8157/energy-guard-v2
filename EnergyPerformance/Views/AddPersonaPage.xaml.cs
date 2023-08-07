using System.Data;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
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
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Core;
using WinRT;

namespace EnergyPerformance.Views;

public sealed partial class AddPersonaPage : Page
{
    const float DEFAULT = 2.0f;

    public PersonaViewModel ViewModel
    {
        get;
    }
    
    public AddPersonaPage()
    {
        ViewModel = App.GetService<PersonaViewModel>();
        InitializeComponent();
    }

    // Function to set the persona slider value to the default setting (2, in this case)
    private void RestoreDefault(object sender, RoutedEventArgs e)
    {
        PersonaSlider.Value = DEFAULT;
    }

    // Function to apply persona to the selected application
    // Grabs the index of the application in the list
    // Sets the corresponding energy value and image path accordingly in the view model
    // Navigates to the Persona Customisation Page
    private void ApplyPersona(object sender, RoutedEventArgs e)
    {
        //var selectedIndex = AppSelection.SelectedIndex;
        var selectedIndex = 0;
        if (selectedIndex != -1)
        {
            var item = ViewModel.PersonasAndRatings[selectedIndex];
            item.EnergyValue = (float)PersonaSlider.Value;
            item.EnergyRating = item.UpdateEnergyRating((int)PersonaSlider.Value);
            Frame.Navigate(typeof(PersonaListPage));
        }
    }

    // Function called when Go Back button is clicked
    // Navigates to the Persona Customisation Page
    private void NavigateToCustomisationPage(object sender, RoutedEventArgs e)
    {
        Frame.Navigate(typeof(PersonaListPage));
    }

    [ComImport, Guid("3E68D4BD-7135-4D10-8018-9FB6D9F33FA1"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IInitializeWithWindow
    {
        void Initialize([In] IntPtr hwnd);
    }

    [DllImport("user32.dll", ExactSpelling = true, CharSet = CharSet.Auto, PreserveSig = true, SetLastError = false)]
    public static extern IntPtr GetActiveWindow();

    private async void TestFilePicker(object sender, RoutedEventArgs e)
    {
        // Open file explorer at the suggested start location
        // With respective filters for file types (in this case .exe)
        FileOpenPicker open = new FileOpenPicker();
        open.SuggestedStartLocation = PickerLocationId.ComputerFolder;
        open.FileTypeFilter.Add(".exe");

        if (Window.Current == null)
        {
            IInitializeWithWindow initializeWithWindowWrapper = open.As<IInitializeWithWindow>();
            IntPtr hwnd = GetActiveWindow();
            initializeWithWindowWrapper.Initialize(hwnd);
        }

        // Opens prompt which allows for selection of file type
        StorageFile file = await open.PickSingleFileAsync();

        if (file != null)
        {
            //debug.AddMessage(file.Name);
            //debug.AddMessage(file.Path);
            // StorageFile can be added to the future access list, wherein it can be then retrieved from with the help of a token
        }
        else
        {
            // Logic for no file chosen
        }

    }

    //// Function called when the selection in the combo box is changed
    //// Gets the selected index from the combo box
    //// Grabs the corresponding energy value, and sets the slider value to it
    //private void UpdateSliderValue(object sender, RoutedEventArgs e)
    //{
    //    var selectedIndex = AppSelection.SelectedIndex;
    //    if (selectedIndex != -1)
    //    {
    //        PersonaSlider.Value = ViewModel.PersonasAndRatings[selectedIndex].EnergyValue;
    //    }
    //}

    //// Overriden OnNavigatedTo - For when a parameter is passed
    //// Updates the form values accordingly, if parameter is passed
    //protected override void OnNavigatedTo(NavigationEventArgs e)
    //{
    //    if (e.Parameter is int && !e.Equals(-1))
    //    {
    //        var index = (int)e.Parameter;
    //        AppSelection.SelectedIndex = index;

    //        PersonaSlider.Value = ViewModel.PersonasAndRatings[index].EnergyValue;  
    //    }
    //    base.OnNavigatedTo(e);
    //}
}