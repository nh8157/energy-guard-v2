using System.Data;
using System.Runtime.InteropServices;
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
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Core;
using WinRT;

namespace EnergyPerformance.Views;

public sealed partial class PersonaCustomisationPage : Page
{

    //public static DebugModel debug;
    public PersonaCustomisationViewModel ViewModel
    {
        get;
    }
    
    public PersonaCustomisationPage()
    {
        ViewModel = App.GetService<PersonaCustomisationViewModel>();
        InitializeComponent();

        //debug = App.GetService<DebugModel>();
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

    #region Select File Code

    [ComImport, Guid("3E68D4BD-7135-4D10-8018-9FB6D9F33FA1"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IInitializeWithWindow
    {
        void Initialize([In] IntPtr hwnd);
    }

    [DllImport("user32.dll", ExactSpelling = true, CharSet = CharSet.Auto, PreserveSig = true, SetLastError = false)]
    public static extern IntPtr GetActiveWindow();

    // Function to pick a file from the file system
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

    #endregion
}