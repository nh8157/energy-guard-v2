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
using System;
using EnergyPerformance.Models;
using LiveChartsCore.Defaults;
using System.Collections.ObjectModel;
using EnergyPerformance.Contracts.Services;
using EnergyPerformance.Views;
using EnergyPerformance.Services;

namespace EnergyPerformance.ViewModels;
public partial class TestMonitorViewModel : ObservableObject
{
    private readonly Random _random = new();
    [ObservableProperty]
    private string currentMode;
    private readonly EnergyUsageModel _model;
    private INavigationService _navigationService;
    public readonly ObservableCollection<String> Applications = new();
    private ColumnSeries<DateTimePoint> historySeries;
    private ColumnSeries<DateTimePoint> costSeries;

    public TestMonitorViewModel()
    {
        Applications.Add("Cost");
        Applications.Add("Energy Usage");
        Applications.Add("Carbon Emission");
        currentMode = "Energy";
        var values = new ObservableCollection<DateTimePoint>();
        var costs = new ObservableCollection<DateTimePoint>();
        _model = App.GetService<EnergyUsageModel>();
        _navigationService = App.GetService<INavigationService>();
        var logs = _model.GetDailyEnergyUsageLogs();
        foreach (var log in logs)
        {
            values.Add(new DateTimePoint(log.Date.Date, log.PowerUsed));
            costs.Add(new DateTimePoint(log.Date.Date, log.Cost));
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
        historySeries.ChartPointPointerDown += OnPointerDown;

        Series = new ISeries[]
        {
            historySeries
        };

        //XAxes = new[] { new Axis() };
    }

    public ISeries[] Series
    {
        get; set;
    }

    public Axis[] XAxes
    {
        get; set;
    } =
{
        new Axis
        {
            Labeler = value => new DateTime((long) value).ToString("MM-dd"),
            //LabelsRotation = 80,

            // when using a date time type, let the library know your unit 
            UnitWidth = TimeSpan.FromDays(1).Ticks, 

            // if the difference between our points is in hours then we would:
            // UnitWidth = TimeSpan.FromHours(1).Ticks,

            // since all the months and years have a different number of days
            // we can use the average, it would not cause any visible error in the user interface
            // Months: TimeSpan.FromDays(30.4375).Ticks
            // Years: TimeSpan.FromDays(365.25).Ticks

            // The MinStep property forces the separator to be greater than 1 day.
            MinStep = TimeSpan.FromDays(1).Ticks
        }
    };


    private void OnPointerDown(IChartView chart, ChartPoint<DateTimePoint, RoundedRectangleGeometry, LabelGeometry>? point)
    {
        if (point?.Visual is null) return;
        //point.Visual.Fill = new SolidColorPaint(SKColors.Red);
        chart.Invalidate(); // <- ensures the canvas is redrawn after we set the fill
        DateTime param = new DateTime((long)point.SecondaryValue);
        _navigationService?.NavigateTo(typeof(MonitorDetailViewModel).FullName, param);
        Debug.Write($"CLick on {point.SecondaryValue}");
    }


    [RelayCommand]
    public void GoToPage1()
    {
        // Get the current date
        DateTime currentDate = DateTime.Now.Date;

        // Calculate the start date for the last seven days
        DateTime startDate = currentDate.AddDays(-6); // Subtract 6 days to get the start date

        // Calculate the end date (today)
        DateTime endDate = currentDate.AddDays(1);

        // Get the ticks for the start and end dates
        long startTicks = startDate.Ticks;
        long endTicks = endDate.Ticks;

        // Update the X-axis limits to display data for the last seven days
        var axis = XAxes[0];
        axis.MinLimit = startTicks;
        axis.MaxLimit = endTicks;
    }


    [RelayCommand]
    public void SeeAll()
    {
        var axis = XAxes[0];
        axis.MinLimit = null;
        axis.MaxLimit = null;
    }

    [RelayCommand]
    public void Cost()
    {
        Debug.WriteLine("On CLick");
        Series = new ISeries[]
        {
            costSeries
        };
        OnPropertyChanged(nameof(Series));

    }
}
