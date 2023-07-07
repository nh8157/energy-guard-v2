using EnergyPerformance.ViewModels;
using Microsoft.UI.Xaml.Controls;

namespace EnergyPerformance.Views;

public sealed partial class DebugPage : Page
{
    public DebugViewModel ViewModel
    {
        get;
    }
    
    public DebugPage()
    {
        ViewModel = App.GetService<DebugViewModel>();
        InitializeComponent();
    }
}