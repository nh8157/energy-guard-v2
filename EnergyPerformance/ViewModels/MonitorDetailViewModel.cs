using System.Collections.Generic;
using System;
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
using Newtonsoft.Json.Linq;
using SkiaSharp;

namespace EnergyPerformance.ViewModels;

public partial class MonitorDetailViewModel : ObservableObject
{
    private readonly EnergyUsageModel _model = App.GetService<EnergyUsageModel>();
    private DateTime _receivedParameter;
    private ColumnSeries<DateTimePoint> historySeries;
    private ColumnSeries<TimeSpanPoint> costSeries;
    private ColumnSeries<TimeSpanPoint> hourlySeries;
    public readonly ObservableCollection<String> DetailApplications = new();
    private string detailSelectedApplication;
    public MonitorDetailViewModel()
    {
        Debug.WriteLine(_model.SelectedModel+"zzzzzzzzzzz");
        
        DetailApplications.Add("Energy Usage");
        DetailApplications.Add("Cost");
        DetailApplications.Add("Carbon Emission");
        
        ReceivedParameter = _model.SelectDate;
        var values = new ObservableCollection<DateTimePoint>();
        var costs = new ObservableCollection<TimeSpanPoint>();
        var hourly = new ObservableCollection<TimeSpanPoint>();
        for (int i = 0; i <= 23; ++i)
        {
            hourly.Add(new TimeSpanPoint(TimeSpan.FromHours(i), 0));
            costs.Add(new TimeSpanPoint(TimeSpan.FromHours(i), 0));
        }
        var logs = _model.GetHourlyEnergyUsageLogs(ReceivedParameter);
        foreach (var log in logs)
        {
            hourly[log.Date.Hour].Value += log.PowerUsed;
            Debug.WriteLine("XXXXXXXXXXXXXXXXXX");
            Debug.WriteLine(log.Date+ "---" + log.PowerUsed);
            values.Add(new DateTimePoint(log.Date, log.PowerUsed));
            costs[log.Date.Hour].Value += log.Cost;
        }
        historySeries = new ColumnSeries<DateTimePoint>
        {
            YToolTipLabelFormatter = (chartPoint) =>
                $"{new DateTime((long)chartPoint.SecondaryValue):HH}H - {chartPoint.PrimaryValue}",
            Name = "Watt",
            Values = values
        };
        costSeries = new ColumnSeries<TimeSpanPoint>
        {
            YToolTipLabelFormatter = (chartPoint) =>
                $"{TimeSpan.FromTicks((long)chartPoint.SecondaryValue).ToString("hh")}H - {chartPoint.PrimaryValue}",
            Name = "Pound",
            Values = costs,
            Fill = new SolidColorPaint(new SKColor(250, 128, 114))
        };
        
        hourlySeries = new ColumnSeries<TimeSpanPoint>
        {
            YToolTipLabelFormatter = (chartPoint) =>
                $"{TimeSpan.FromTicks((long)chartPoint.SecondaryValue).ToString("hh")}H - {chartPoint.PrimaryValue}",
            Name = "Watt",
            Values = hourly,
            Fill = new SolidColorPaint(new SKColor(51, 181, 255))
        };
        SeriesHourly = new ISeries[]
        {
            hourlySeries
        };
        SeriesCostHourly = new ISeries[]
        {
            costSeries
        };

        GotoPage();
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

    public ISeries[] SeriesCostHourly
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
            Labeler = value => TimeSpan.FromTicks((long)value).ToString("HH"),
            //LabelsRotation = 80,

            // when using a date time type, let the library know your unit 
            UnitWidth = TimeSpan.FromHours(1).Ticks, 

            // if the difference between our points is in hours then we would:
            // UnitWidth = TimeSpan.FromHours(1).Ticks,

            // since all the months and years have a different number of days
            // we can use the average, it would not cause any visible error in the user interface
            // Months: TimeSpan.FromDays(30.4375).Ticks
            // Years: TimeSpan.FromDays(365.25).Ticks

            // The MinStep property forces the separator to be greater than 1 day.
            MinStep = TimeSpan.FromHours(1).Ticks,
        }
    };

    public Axis[] XXAxes
    {
        get; set;

    } =
 {
        new Axis
        {
            Labeler = value =>TimeSpan.FromTicks((long)value).ToString("hh"),
            //LabelsRotation = 80, 

            // when using a date time type, let the library know your unit 
            UnitWidth = TimeSpan.FromHours(1).Ticks, 

            // if the difference between our points is in hours then we would:
            // UnitWidth = TimeSpan.FromHours(1).Ticks,

            // since all the months and years have a different number of days
            // we can use the average, it would not cause any visible error in the user interface
            // Months: TimeSpan.FromDays(30.4375).Ticks
            // Years: TimeSpan.FromDays(365.25).Ticks

            // The MinStep property forces the separator to be greater than 1 day.
            MinStep = TimeSpan.FromHours(1).Ticks,
        }
    };

    public void GotoPage()
    {
        var axis = XAxes[0];
        axis.MinLimit = ReceivedParameter.Ticks;
        axis.MaxLimit = new DateTime(ReceivedParameter.Year, ReceivedParameter.Month, ReceivedParameter.Day, 23, 59, 59).Ticks;
    }

    public string DetailSelectedApplication
    {
        get
        {
            return detailSelectedApplication;
        }
        set
        {
            if (detailSelectedApplication != value)
            {
                detailSelectedApplication = value;
                OnPropertyChanged(nameof(detailSelectedApplication));
            }
        }
    }

    public string SelectedChoiceModel
    {
        get
        {
            return _model.SelectedModel;
        }
        set
        {
            if (_model.SelectedModel != value)
            {
                _model.SelectedModel = value;
                OnPropertyChanged(nameof(SelectedChoiceModel));
            }
        }
    }
}