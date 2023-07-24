using System.Collections.Generic;
using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using EnergyPerformance.Contracts.ViewModels;
using LiveChartsCore;
using LiveChartsCore.Measure;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using Microsoft.UI.Xaml.Navigation;
using SkiaSharp;

namespace EnergyPerformance.ViewModels;

public partial class MonitorDetailViewModel : ObservableObject, INavigationAware
{
    private string _receivedParameter;

    public string ReceivedParameter
    {
        get => _receivedParameter;
        set => SetProperty(ref _receivedParameter, value);
    }

    public ISeries[] Series
    {
        get; set;
    } =
    {
        new RowSeries<int>
        {
            Values = new List<int> { 8, 3, 4,10,11,12,13 },
            Stroke = null,
            DataLabelsPaint = new SolidColorPaint(new SKColor(45, 45, 45)),
            DataLabelsSize = 14,
            DataLabelsPosition = DataLabelsPosition.End
        }
    };

    public ISeries[] SeriesHour
    {
        get; set;
    } =
    {
        new ColumnSeries<double>
        {
            Name = "Mary",
            Values = new double[] { 2, 5, 4 }
        },
        new ColumnSeries<double>
        {
            Name = "Ana",
            Values = new double[] { 3, 1, 6 }
        }
    };

    public Axis[] XAxes
    {
        get; set;
    } =
    {
        new Axis
        {
            Labels = new string[] { "Category 1", "Category 2", "Category 3" },
            LabelsRotation = 0,
            SeparatorsPaint = new SolidColorPaint(new SKColor(200, 200, 200)),
            SeparatorsAtCenter = false,
            TicksPaint = new SolidColorPaint(new SKColor(35, 35, 35)),
            TicksAtCenter = true
        }
    };

    public void OnNavigatedFrom()
    {
    }
    public void OnNavigatedTo(object parameter)
    {
        ReceivedParameter = parameter?.ToString() ?? "No parameter received"; // Set the ReceivedParameter property with the value passed from TestMonitorViewModel
        Debug.WriteLine($"Received Parameter: {ReceivedParameter}");
    }
}