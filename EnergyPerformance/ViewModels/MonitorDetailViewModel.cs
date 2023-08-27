using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing;
using System.Reflection.Emit;
using CommunityToolkit.Mvvm.ComponentModel;
using EnergyPerformance.Models;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.Drawing;
using LiveChartsCore.Measure;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;

namespace EnergyPerformance.ViewModels;


public partial class MonitorDetailViewModel : ObservableObject
{
    private readonly EnergyUsageModel _model;
    private readonly ColumnSeries<DateTimePoint> _historySeries;
    private readonly ColumnSeries<TimeSpanPoint> _costSeries;
    private readonly ColumnSeries<TimeSpanPoint> _carbonSeries;
    private readonly ColumnSeries<TimeSpanPoint> _hourlySeries;
    private readonly RowSeries<float> _rowSeries;
    private readonly RowSeries<float> _rowCostSeries;
    private readonly RowSeries<float> _rowCarbonSeries;
    public readonly ObservableCollection<String> DetailApplications = new();

    private DateTime _receivedParameter;
    private string _detailSelectedApplication;
    public MonitorDetailViewModel()
    {
        DetailApplications.Add("Energy Usage");
        DetailApplications.Add("Cost");
        DetailApplications.Add("Carbon Emission");

        // initialize the field to some default value
        _detailSelectedApplication = "Energy Usage";

        _model = App.GetService<EnergyUsageModel>();
        ReceivedParameter = _model.SelectedDate;
        var values = new ObservableCollection<DateTimePoint>();
        var costs = new ObservableCollection<TimeSpanPoint>();
        var hourly = new ObservableCollection<TimeSpanPoint>();
        var carbons = new ObservableCollection<TimeSpanPoint>();
        var rowEnergyUsage = new List<float>();
        var rowCost = new List<float>();
        var rowCarbonUsage = new List<float>();
        //the for loop represents the 24 hour period
        for (var i = 0; i <= 23; ++i)
        {
            hourly.Add(new TimeSpanPoint(TimeSpan.FromHours(i), 0));
            costs.Add(new TimeSpanPoint(TimeSpan.FromHours(i), 0));
            carbons.Add(new TimeSpanPoint(TimeSpan.FromHours(i), 0));
        }
        var logs = _model.GetHourlyEnergyUsageLogs(ReceivedParameter);
        var perAppLog = _model.GetPerAppUsageLogs(ReceivedParameter);
        string[] stringArray = new string[8];
        var index = 0;
        foreach (var papp in perAppLog)
        {
            if (index > 8) break;
            //rowEnergyUSage.Add((float)Math.Round(papp.Item2.PowerUsed, 6));
            if (papp.Item2.PowerUsed != 0.0f)
            {
                rowEnergyUsage.Add(papp.Item2.PowerUsed +1);
                rowCost.Add(papp.Item2.Cost + 1);
                rowCarbonUsage.Add(papp.Item2.CarbonEmission + 1);
                stringArray[index++] = papp.Item1;
            }
        }


        foreach (var log in logs)
        {
            hourly[log.Date.Hour].Value += log.PowerUsed;
            values.Add(new DateTimePoint(log.Date, log.PowerUsed));
            costs[log.Date.Hour].Value += log.Cost;
            carbons[log.Date.Hour].Value += log.CarbonEmission;
        }
        _historySeries = new ColumnSeries<DateTimePoint>
        {
            //show the text when hover the bar, it shows the hour and the value
            YToolTipLabelFormatter = (chartPoint) =>
                $"{new DateTime((long)chartPoint.SecondaryValue):HH}H - {chartPoint.PrimaryValue.ToString("F4")}",
            Name = "Watt",
            Values = values
        };
        _costSeries = new ColumnSeries<TimeSpanPoint>
        {
            //show the text when hover the bar, it shows the hour and the value
            YToolTipLabelFormatter = (chartPoint) =>
                $"\u00A3{chartPoint.PrimaryValue.ToString("F4")}",
            Name = "Cost",
            Values = costs,
            Fill = new SolidColorPaint(new SKColor(250, 128, 114))
        };

        _hourlySeries = new ColumnSeries<TimeSpanPoint>
        {
            //show the text when hover the bar, it shows the hour and the value
            YToolTipLabelFormatter = (chartPoint) =>
                $"{chartPoint.PrimaryValue.ToString("F4")} watt",
            Name = "Watt",
            Values = hourly,
            Fill = new SolidColorPaint(new SKColor(51, 181, 255))
        };
        _carbonSeries = new ColumnSeries<TimeSpanPoint>
        {
            //show the text when hover the bar, it shows the hour and the value
            YToolTipLabelFormatter = (chartPoint) =>
                $"{chartPoint.PrimaryValue.ToString("F4")} g",
            Name = "CO2",
            Values = carbons,
            Fill = new SolidColorPaint(new SKColor(144, 238, 144))
        };
        _rowSeries = new RowSeries<float>
        {
            Values = rowEnergyUsage,
            //Values = new List<float> { 8,1,2,3,4,1.2f},
            Stroke = null,
            DataLabelsPaint = new SolidColorPaint(new SKColor(45,45, 45)),
            //DataLabelsSize = 10,
            DataLabelsPosition = DataLabelsPosition.End,
            DataLabelsTranslate = new LvcPoint(-1, 0),
            DataLabelsFormatter = point => $"{""}",
            YToolTipLabelFormatter = (chartPoint) =>
                  $"{chartPoint.PrimaryValue -1} watt",
            MaxBarWidth = 28,
            Name = "Watt",
            Fill = new SolidColorPaint(new SKColor(51, 181, 255))

        };
        _rowCostSeries = new RowSeries<float>
        {
            Values = rowCost,
            //Values = new List<float> { 8,1,2,3,4,1.2f},
            Stroke = null,
            DataLabelsPaint = new SolidColorPaint(new SKColor(45, 45, 45)),
            //DataLabelsSize = 10,
            DataLabelsPosition = DataLabelsPosition.End,
            DataLabelsTranslate = new LvcPoint(-1, 0),
            DataLabelsFormatter = point => $"{""}",
            YToolTipLabelFormatter = (chartPoint) =>
                  $"\u00A3{chartPoint.PrimaryValue - 1}",
            MaxBarWidth = 28,
            Name = "Cost",
            Fill = new SolidColorPaint(new SKColor(250, 128, 114))
        };
        _rowCarbonSeries = new RowSeries<float>
        {
            Values = rowCarbonUsage,
            //Values = new List<float> { 8,1,2,3,4,1.2f},
            Stroke = null,
            DataLabelsPaint = new SolidColorPaint(new SKColor(45, 45, 45)),
            //DataLabelsSize = 10,
            DataLabelsPosition = DataLabelsPosition.End,
            DataLabelsTranslate = new LvcPoint(-1, 0),
            DataLabelsFormatter = point => $"{""}",
            YToolTipLabelFormatter = (chartPoint) =>
                  $"{chartPoint.PrimaryValue - 1} g",
            MaxBarWidth = 28,
            Name = "CO2",
            Fill = new SolidColorPaint(new SKColor(144, 238, 144))

        };

        SeriesHourly = new ISeries[]
        {
            _hourlySeries
        };
        SeriesCostHourly = new ISeries[]
        {
            _costSeries
        };
        SeriesCarbonHourly = new ISeries[]
        {
            _carbonSeries
        };
        Series = new ISeries[]
        {
            _rowSeries
        };
        CostSeries = new ISeries[]
        {
            _rowCostSeries
        };
        CarbonSeries = new ISeries[]
        {
            _rowCarbonSeries
        };
        PerAppAxis = new Axis[]
        {
            new Axis
            {
                Labels = stringArray,
                ForceStepToMin = true,
            }
        };
        PerAppXAxis = new Axis[]
        {
            new Axis
            {
                IsVisible = false,

            }
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
    }
    public ISeries[] CostSeries
    {
        get; set;
    }
    public ISeries[] CarbonSeries
    {
        get; set;
    }

    public ISeries[] SeriesHourly
    {
        get; set;
    }

    public ISeries[] SeriesCostHourly
    {
        get; set;
    }
    public ISeries[] SeriesCarbonHourly
    {
        get; set;
    }

    public Axis[] PerAppAxis
    {
    get; set; }

    public Axis[] PerAppXAxis
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
            ForceStepToMin = true
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
            return _detailSelectedApplication;
        }
        set
        {
            if (_detailSelectedApplication != value)
            {
                _detailSelectedApplication = value;
                OnPropertyChanged(nameof(_detailSelectedApplication));
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