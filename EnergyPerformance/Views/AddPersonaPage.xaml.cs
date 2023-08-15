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
using Shell32;
using Windows.Media.AppRecording;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Core;
using WinRT;
using WinRT.Interop;

namespace EnergyPerformance.Views;

public sealed partial class AddPersonaPage : Page
{
    const float DEFAULT = 2.0f;
    private static string CurrentPath = "";

    public PersonaViewModel ViewModel
    {
        get;
    }
    
    public AddPersonaPage()
    {
        ViewModel = App.GetService<PersonaViewModel>();
        InitializeComponent();
    }

    // Function called when Add Persona is clicked
    // If a valid app is selected, add the values to the persona list
    // Navigate back to the Persona List Page
    private void AddPersona(object sender, RoutedEventArgs e)
    {
        if (AppSelection.Text != null && AppSelection.Text != "")
        {
            ViewModel.Add(AppSelection.Text, (float) PersonaSlider.Value);
            Frame.Navigate(typeof(PersonaListPage));
        }
    }

    // Function called when Go Back button is clicked
    // Navigates to the Persona List Page
    private void NavigateToListPage(object sender, RoutedEventArgs e)
    {
        Frame.Navigate(typeof(PersonaListPage));
    }

    #region Select File Code
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
            CurrentPath = Marshal.PtrToStringUni(path); // Store the path of the object in CurrentPath

            if (CurrentPath != null && CurrentPath != "" && CurrentPath.Substring(CurrentPath.Length - 4).Equals(".lnk"))
            {
                CurrentPath = GetTargetExecutable(CurrentPath);
            }
            if (CurrentPath != null && CurrentPath != "" && CurrentPath.Substring(CurrentPath.Length - 4).Equals(".exe"))
            {
                var AppName = GetApplicationName(CurrentPath);
                AppSelection.Text = AppName;
            }
        }
    }

    // Function to get the Target Executable, if the path supplied points to a shortcut
    private string GetTargetExecutable(string path)
    {
        var filepath = Path.GetDirectoryName(path);
        var filename = Path.GetFileName(path);

        Shell shell = new Shell();
        Folder folder = shell.NameSpace(filepath);
        FolderItem folderItem = folder.ParseName(filename);
        if (folderItem != null && folderItem.IsLink)
        {
            ShellLinkObject link = (ShellLinkObject)folderItem.GetLink;
            return link.Path;
        }
        return string.Empty;
    }

    // Function to get the Application Name from the string path
    // Get the substring after the last occurrence of \ and return
    private string GetApplicationName(string str)
    {
        var index = str.LastIndexOf('\\') + 1;
        return str.Substring(index);
    }

    #endregion
}