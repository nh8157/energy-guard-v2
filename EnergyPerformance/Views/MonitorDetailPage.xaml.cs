using System.Diagnostics;
using EnergyPerformance.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace EnergyPerformance.Views;

public sealed partial class MonitorDetailPage : Page
{
    public MonitorDetailViewModel ViewModel
    {
        get;
    }

    public MonitorDetailPage()
    {
        ViewModel = App.GetService<MonitorDetailViewModel>();
        InitializeComponent();
    }

    

    private void ModelSelection_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var selectedValue = ViewModel.DetailSelectedApplication;
        if (selectedValue.Equals("Cost"))
        {
            LvcChart.Series = ViewModel.SeriesCostHourly;
            rowChart.Series = ViewModel.CostSeries;
        }

        else if (selectedValue.Equals("Energy Usage")) {
            LvcChart.Series = ViewModel.SeriesHourly;
            rowChart.Series = ViewModel.Series;
        }

        else
        {
            LvcChart.Series = ViewModel.SeriesCarbonHourly;
            rowChart.Series = ViewModel.CarbonSeries;
        }
            
    }

    private void NavigateToCustomisationPage(object sender, RoutedEventArgs e)
    {
        Frame.Navigate(typeof(HistoryPage));
    }

    private void OnLoaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        var selectedValue = ViewModel.SelectedChoiceModel;
        ModelSelection.SelectedValue = selectedValue;
        if (selectedValue.Equals("Cost"))
        {
            LvcChart.Series = ViewModel.SeriesCostHourly;
            LvcChart.Series = ViewModel.CostSeries;
        }

        else if (selectedValue.Equals("Energy Usage"))
        {
            LvcChart.Series = ViewModel.SeriesHourly;
            rowChart.Series = ViewModel.Series;
        }
        else
        {
            LvcChart.Series = ViewModel.SeriesCarbonHourly;
            rowChart.Series = ViewModel.CarbonSeries;
        }
            
    }
}