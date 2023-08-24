using System.Diagnostics;
using EnergyPerformance.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Navigation;

namespace EnergyPerformance.Views;

public sealed partial class HistoryPage : Page
{
    public HistoryViewModel ViewModel
    {
        get;
    }

    public HistoryPage()
    {
        ViewModel = App.GetService<HistoryViewModel>();
        InitializeComponent();
    }


    private void ModelSelection_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var selectedValue = ViewModel.SelectedApplication;
        //// ViewModel.SomeFunction(selectedValue);
        //ViewModel.ModelChanged(selectedValue);
        //Debug.WriteLine(ViewModel.Series.ToString());
        if (selectedValue.Equals("Cost"))
        {
            LvcChart.Series = ViewModel.CostSeries;
        }
            
        else if(selectedValue.Equals("Energy Usage"))
        {
            LvcChart.Series = ViewModel.HistorySeries;
        }
            
        else if(selectedValue.Equals("Carbon Emission"))
        {
            LvcChart.Series = ViewModel.CarbonSeries;
        }
            
    }



}