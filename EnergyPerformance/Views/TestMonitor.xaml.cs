using EnergyPerformance.ViewModels;
using Microsoft.UI.Xaml.Controls;

namespace EnergyPerformance.Views;

public sealed partial class TestMonitorPage : Page
{
    public TestMonitorViewModel ViewModel
    {
        get;
    }

    public TestMonitorPage()
    {
        ViewModel = App.GetService<TestMonitorViewModel>();
        InitializeComponent();
    }
}