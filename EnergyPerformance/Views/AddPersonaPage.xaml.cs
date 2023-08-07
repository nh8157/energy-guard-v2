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
using WinRT.Interop;

namespace EnergyPerformance.Views;

public sealed partial class AddPersonaPage : Page
{
    public static DebugModel debug;
    const float DEFAULT = 2.0f;

    public PersonaViewModel ViewModel
    {
        get;
    }
    
    public AddPersonaPage()
    {
        ViewModel = App.GetService<PersonaViewModel>();
        InitializeComponent();

        debug = App.GetService<DebugModel>();
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

    [DllImport("Shell32.dll", CharSet = CharSet.Unicode)]
    public static extern IntPtr SHBrowseForFolder(ref BROWSEINFO lpbi);

    [DllImport("Shell32.dll", CharSet = CharSet.Unicode)]
    public static extern bool SHGetPathFromIDList(IntPtr pidl, IntPtr pszPath);

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct BROWSEINFO
    {
        public IntPtr Owner;
        public IntPtr PIDL;
        public IntPtr DisplayName;
        public string Title;
        public uint Flags;
        public IntPtr Callback;
        public IntPtr Param;
        public int Image;
    }

    public const uint BIF_NEWDIALOGSTYLE = 0x00000040;
    public const uint BIF_USENEWUI = BIF_NEWDIALOGSTYLE | 0x00000200;
    public const uint BIF_BROWSEINCLUDEFILES = 0x00004000;

    private void BrowseClicked(object sender, RoutedEventArgs e)
    {
        var bi = new BROWSEINFO
        {
            Title = "Select Application",
            Flags = BIF_NEWDIALOGSTYLE | BIF_USENEWUI | BIF_BROWSEINCLUDEFILES,
        };

        var pidl = SHBrowseForFolder(ref bi);
        if (pidl != IntPtr.Zero)
        {
            var path = Marshal.AllocHGlobal(260);
            SHGetPathFromIDList(pidl, path);
            Marshal.FreeHGlobal(path);
            var pathString = Marshal.PtrToStringUni(path);
            //pathString is the application's path

            if (pathString != null && pathString.Substring(pathString.Length - 4).Equals(".exe"))
            {
                var AppName = GetApplicationName(pathString);
                AppSelection.Text = AppName;
            }
        }
    }

    private string GetApplicationName(string str)
    {
        var index = str.LastIndexOf('\\') + 1;
        return str.Substring(index);
    }

}