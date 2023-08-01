using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EnergyPerformance.Contracts.ViewModels;
using EnergyPerformance.Models;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.Measure;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using Microsoft.Extensions.Hosting;
using Microsoft.UI.Xaml.Navigation;
using SkiaSharp;

namespace EnergyPerformance.ViewModels;

public partial class MonitorDetailViewModel : ObservableObject
{
    private readonly EnergyUsageModel _model = App.GetService<EnergyUsageModel>();
    private DateTime _receivedParameter;
    private ColumnSeries<DateTimePoint> historySeries;
    private ColumnSeries<DateTimePoint> costSeries;

    public MonitorDetailViewModel()
    {
        ReceivedParameter = _model.SelectDate;
        var values = new ObservableCollection<DateTimePoint>();
        var costs = new ObservableCollection<DateTimePoint>();
        Debug.WriteLine(ReceivedParameter.ToString()+"xxxx");
        var logs = _model.GetHourlyEnergyUsageLogs(ReceivedParameter);
        if(logs != null)
        {
            Debug.WriteLine(logs.Count);
        }
        foreach (var log in logs)
        {
            Debug.WriteLine("XXXXXXXXXXXXXXXXXX");
            Debug.WriteLine(log.Date+ "---" + log.PowerUsed);
            values.Add(new DateTimePoint(log.Date, log.PowerUsed));
            costs.Add(new DateTimePoint(log.Date, log.Cost));
        }
        historySeries = new ColumnSeries<DateTimePoint>
        {
            YToolTipLabelFormatter = (chartPoint) =>
                $"{new DateTime((long)chartPoint.SecondaryValue):MM-dd}: {chartPoint.PrimaryValue.ToString("F2")}",
            Name = "Watt",
            Values = values
        };
        costSeries = new ColumnSeries<DateTimePoint>
        {
            YToolTipLabelFormatter = (chartPoint) =>
                $"{new DateTime((long)chartPoint.SecondaryValue):MM-dd}: {chartPoint.PrimaryValue.ToString("F2")}",
            Name = "Pound",
            Values = costs
        };
        SeriesHourly = new ISeries[]
        {
            historySeries
        };
        XAxes = new[] { new Axis() };
    }
    

    
    public DateTime ReceivedParameter
    {
        get => _receivedParameter;
        set
        {
            if (_receivedParameter != value)
            {
                _receivedParameter = value;
                OnPropertyChanged(nameof(ReceivedParameter));
            }
        }
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

    public ISeries[] SeriesHourly
    {
        get; set;
    }

    public Axis[] XAxes
    {
        get; set;
    }

    //public void OnNavigatedFrom()
    //{
    //}
    //public void OnNavigatedTo(object parameter)
    //{
    //    ReceivedParameter = parameter?.ToString() ?? "No parameter received"; // Set the ReceivedParameter property with the value passed from TestMonitorViewModel
    //    Debug.WriteLine($"Received Parameter: {ReceivedParameter}");
    //}

}