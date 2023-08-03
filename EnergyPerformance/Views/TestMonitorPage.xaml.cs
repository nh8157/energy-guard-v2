using System.Diagnostics;
using EnergyPerformance.ViewModels;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
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


    private void ModelSelection_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        // 在这里处理下拉框值改变的逻辑
        // 获取选中的值
        var selectedValue = ViewModel.SelectedApplication;
        //Debug.WriteLine(selectedValue);
        //// 执行你想要执行的函数
        //// 例如：ViewModel.SomeFunction(selectedValue);
        //ViewModel.ModelChanged(selectedValue);
        //Debug.WriteLine(ViewModel.Series.ToString());
        if (selectedValue.Equals("Cost"))
        {
            LvcChart.Series = ViewModel.CostSeries;
        }
            
        else if(selectedValue.Equals("Energy Usage"))
            LvcChart.Series = ViewModel.Series;
        else
            LvcChart.Series = ViewModel.Series;
    }

}