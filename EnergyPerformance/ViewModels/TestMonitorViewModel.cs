using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Diagnostics;
using LiveChartsCore;
using EnergyPerformance.Helpers;
using EnergyPerformance.Models;
using LiveChartsCore.Kernel;
using LiveChartsCore.Kernel.Sketches;
using LiveChartsCore.Measure;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Drawing.Geometries;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using EnergyPerformance.Models;

namespace EnergyPerformance.ViewModels;
public partial class TestMonitorViewModel : ObservableObject
{
    private readonly Random _random = new();
    private readonly EnergyUsageModel _model;

    public TestMonitorViewModel()
    {
       
        var values = new List<float>();
        _model = App.GetService<EnergyUsageModel>();
        var logs = _model.GetDailyEnergyUsageLogs();
        for (int i=0;i<=30;++i)
        {
            if (i < logs.Count) values.Add(logs[i].PowerUsed);
            else values.Add(0);
            Debug.WriteLine(values[i]);
          
        }
        var historySeries = new ColumnSeries<float>
        {
            Values = values
        };
        historySeries.ChartPointPointerDown += OnPointerDown;

        Series = new ISeries[]
        {
            historySeries
        };

        XAxes = new[] { new Axis() };
        Debug.WriteLine("123");
    }

    public ISeries[] Series
    {
        get;
    }


    private void OnPointerDown(IChartView chart, ChartPoint<float, RoundedRectangleGeometry, LabelGeometry>? point)
    {
        if (point?.Visual is null) return;
        //point.Visual.Fill = new SolidColorPaint(SKColors.Red);
        chart.Invalidate(); // <- ensures the canvas is redrawn after we set the fill
        Debug.Write($"CLick on {point.SecondaryValue}");
    }

    public Axis[] XAxes
    {
        get;
    }

    [RelayCommand]
    public void GoToPage1()
    {
        var axis = XAxes[0];
        axis.MinLimit = 19.5;
        axis.MaxLimit = 30.5;
    }

    [RelayCommand]
    public void GoToPage2()
    {
        var axis = XAxes[0];
        axis.MinLimit = 9.5;
        axis.MaxLimit = 20.5;
    }

    [RelayCommand]
    public void GoToPage3()
    {
        var axis = XAxes[0];
        axis.MinLimit = 19.5;
        axis.MaxLimit = 30.5;
    }

    [RelayCommand]
    public void SeeAll()
    {
        var axis = XAxes[0];
        axis.MinLimit = null;
        axis.MaxLimit = null;
    }
}
