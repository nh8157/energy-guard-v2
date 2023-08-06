﻿using CommunityToolkit.Mvvm.ComponentModel;
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
using System.ComponentModel;

namespace EnergyPerformance.ViewModels;
public partial class HistoryViewModel : ObservableObject
{
    private readonly Random _random = new();
    private readonly EnergyUsageModel _model;
    private INavigationService _navigationService;
    public readonly ObservableCollection<String> Applications = new();
    private ColumnSeries<DateTimePoint> historySeries;
    private ColumnSeries<DateTimePoint> costSeries;
    private ColumnSeries<DateTimePoint> carbonSeries;
    private string selectedApplication;
    private DateTime lastDate = DateTime.Today;
    private DateTime currentStartDate = DateTime.Today.AddDays(-6.5);

    public HistoryViewModel()
    {
        Applications.Add("Energy Usage");
        Applications.Add("Cost");
        Applications.Add("Carbon Emission");
        var values = new ObservableCollection<DateTimePoint>();
        var costs = new ObservableCollection<DateTimePoint>();
        var carbons = new ObservableCollection<DateTimePoint>();
        _model = App.GetService<EnergyUsageModel>();
        _model.SelectedModel = "Energy Usage";
        _navigationService = App.GetService<INavigationService>();
        var logs = _model.GetDailyEnergyUsageLogs();
        foreach (var log in logs)
        {
            if (lastDate.CompareTo(log.Date) > 0 && log.PowerUsed>0)
            {
                lastDate = log.Date;
            }
            values.Add(new DateTimePoint(log.Date.Date, log.PowerUsed));
            costs.Add(new DateTimePoint(log.Date.Date, log.Cost));
            carbons.Add(new DateTimePoint(log.Date.Date, log.CarbonEmission));
        }
        historySeries = new ColumnSeries<DateTimePoint>
        {
            //shows the text when hovering the bar
            YToolTipLabelFormatter = (chartPoint) =>
                $"{new DateTime((long)chartPoint.SecondaryValue):MM/dd}: {chartPoint.PrimaryValue.ToString("F4")}",
            Name = "Watt",
            Values = values,
            Fill = new SolidColorPaint(new SKColor(51, 181, 255))
        };
        costSeries = new ColumnSeries<DateTimePoint>
        {
            //shows the text when hovering the bar
            YToolTipLabelFormatter = (chartPoint) =>
                $"{new DateTime((long)chartPoint.SecondaryValue):MM/dd}: {chartPoint.PrimaryValue.ToString("F4")}",
            Name = "Pound",
            Values = costs,
            Fill = new SolidColorPaint(new SKColor(250, 128, 114))
        };
        carbonSeries = new ColumnSeries<DateTimePoint>
        {
            //shows the text when hovering the bar
            YToolTipLabelFormatter = (chartPoint) =>
                $"{new DateTime((long)chartPoint.SecondaryValue):MM/dd}: {chartPoint.PrimaryValue.ToString("F4")}",
            Name = "CO2",
            Values = carbons,
            Fill = new SolidColorPaint(new SKColor(143, 188, 143))
        };
        historySeries.ChartPointPointerDown += OnPointerDown;
        costSeries.ChartPointPointerDown += OnCostPointerDown;
        carbonSeries.ChartPointPointerDown += OnCarbonPointerDown;
        HistorySeries = new ISeries[]
        {
            historySeries
        };

        CostSeries = new ISeries[]
       {
            costSeries
       };

        CarbonSeries = new ISeries[]
       {
            carbonSeries
       };
        GoToPage();
    }

    public ISeries[] HistorySeries
    {
        get; set;
    }

    public ISeries[] CostSeries
    {
        get; set;
    }

    public ISeries[] CarbonSeries
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
            Labeler = value => new DateTime((long) value).ToString("MM/dd"),
            UnitWidth = TimeSpan.FromDays(1).Ticks, 
            MinStep = TimeSpan.FromDays(1).Ticks
        }
    };

    public Axis[] YAxes
    {
        get; set;

    } =
{
        new Axis
        {
            Labeler = value => value.ToString("F4"),
            
        }
    };


    private void OnPointerDown(IChartView chart, ChartPoint<DateTimePoint, RoundedRectangleGeometry, LabelGeometry>? point)
    {
        if (point?.Visual is null) return;
        chart.Invalidate(); // <- ensures the canvas is redrawn after we set the fill
        DateTime param = new DateTime((long)point.SecondaryValue);
        _model.SelectDate = param;
        _model.SelectedModel = "Energy Usage";
        _navigationService?.NavigateTo(typeof(MonitorDetailViewModel).FullName);
    }

    private void OnCostPointerDown(IChartView chart, ChartPoint<DateTimePoint, RoundedRectangleGeometry, LabelGeometry>? point)
    {
        if (point?.Visual is null) return;
        chart.Invalidate(); // <- ensures the canvas is redrawn after we set the fill
        DateTime param = new DateTime((long)point.SecondaryValue);
        _model.SelectDate = param;
        _model.SelectedModel = "Cost";
        _navigationService?.NavigateTo(typeof(MonitorDetailViewModel).FullName);
    }

    private void OnCarbonPointerDown(IChartView chart, ChartPoint<DateTimePoint, RoundedRectangleGeometry, LabelGeometry>? point)
    {
        if (point?.Visual is null) return;
        chart.Invalidate(); // <- ensures the canvas is redrawn after we set the fill
        DateTime param = new DateTime((long)point.SecondaryValue);
        _model.SelectDate = param;
        _model.SelectedModel = "Carbon Emission";
        _navigationService?.NavigateTo(typeof(MonitorDetailViewModel).FullName);
    }

    public void GoToPage()
    {
        // Get the current date
        DateTime currentDate = DateTime.Now.Date;

        // Calculate the start date for the last seven days
        DateTime startDate = currentDate.AddDays(-6.5); // Subtract 6 days to get the start date

        // Calculate the end date (today)
        DateTime endDate = currentDate.AddDays(0.5);

        // Get the ticks for the start and end dates
        long startTicks = startDate.Ticks;
        long endTicks = endDate.Ticks;

        // Update the X-axis limits to display data for the last seven days
        var axis = XAxes[0];
        axis.MinLimit = startTicks;
        axis.MaxLimit = endTicks;
    }

    [RelayCommand]
    public void LastWeek()
    {
        // Calculate the start date for the last seven days
        DateTime startDate = currentStartDate.AddDays(-7); // Subtract 6 days to get the start date

        // Calculate the end date (today)
        DateTime endDate = currentStartDate;

        if(endDate.CompareTo(lastDate) < 0)
        {
            return;
        }

        // Get the ticks for the start and end dates
        long startTicks = startDate.Ticks;
        long endTicks = endDate.Ticks;
        currentStartDate = startDate;

        // Update the X-axis limits to display data for the last seven days
        var axis = XAxes[0];
        axis.MinLimit = startTicks;
        axis.MaxLimit = endTicks;
    }


    [RelayCommand]
    public void NextWeek()
    {
        DateTime startDate = currentStartDate.AddDays(7); // Subtract 6 days to get the start date

        // Calculate the end date (today)
        DateTime endDate = currentStartDate.AddDays(14);

        if (endDate.CompareTo(DateTime.Today.AddDays(1)) > 0)
        {
            return;
        }

        // Get the ticks for the start and end dates
        long startTicks = startDate.Ticks;
        long endTicks = endDate.Ticks;
        currentStartDate = startDate;

        // Update the X-axis limits to display data for the last seven days
        var axis = XAxes[0];
        axis.MinLimit = startTicks;
        axis.MaxLimit = endTicks;
    }


    public string SelectedApplication
    {
        get
        {
            return selectedApplication;
        }
        set
        {
            if (selectedApplication != value)
            {
                selectedApplication = value;
                OnPropertyChanged(nameof(SelectedApplication));
            }
        }
    }
}