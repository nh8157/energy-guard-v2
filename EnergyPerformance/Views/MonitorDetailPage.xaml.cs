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
        Debug.WriteLine("xxx");
    }

    

    private void ModelSelection_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        // 在这里处理下拉框值改变的逻辑
        // 获取选中的值
        var selectedValue = ViewModel.DetailSelectedApplication;
        //Debug.WriteLine(selectedValue);
        //// 执行你想要执行的函数
        //// 例如：ViewModel.SomeFunction(selectedValue);
        //ViewModel.ModelChanged(selectedValue);
        //Debug.WriteLine(ViewModel.Series.ToString());
        if (selectedValue.Equals("Cost"))
            LvcChart.Series = ViewModel.SeriesCostHourly;
        else if (selectedValue.Equals("Energy Usage"))
            LvcChart.Series = ViewModel.SeriesHourly;
        else
            LvcChart.Series = ViewModel.SeriesCarbonHourly;
    }

    private void NavigateToCustomisationPage(object sender, RoutedEventArgs e)
    {
        Frame.Navigate(typeof(TestMonitorPage));
    }

    private void OnLoaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        var selectedValue = ViewModel.SelectedChoiceModel;
        ModelSelection.SelectedValue = selectedValue;
        if (selectedValue.Equals("Cost"))
        {
            LvcChart.Series = ViewModel.SeriesCostHourly;
        }

        else if (selectedValue.Equals("Energy Usage"))
            LvcChart.Series = ViewModel.SeriesHourly;
        else
            LvcChart.Series = ViewModel.SeriesCarbonHourly;
    }

    }