using System.Diagnostics;
using EnergyPerformance.ViewModels;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

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

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        if (e.Parameter != null)
        {
            // Retrieve the parameter and cast it to the appropriate type
            string parameterValue = (string)e.Parameter;
            Debug.WriteLine(parameterValue);

            // Now you have the parameterValue, do whatever you need to with it
        }
    }

}