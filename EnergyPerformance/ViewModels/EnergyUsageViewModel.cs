using System.Diagnostics;
using System.Reflection;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EnergyPerformance.Core.Helpers;
using EnergyPerformance.Helpers;
using EnergyPerformance.Models;
using OxyPlot;
using OxyPlot.Annotations;
using OxyPlot.Axes;
using OxyPlot.Series;

namespace EnergyPerformance.ViewModels;

/// <summary>
/// ViewModel for the Energy Usage page.
/// </summary>
public partial class EnergyUsageViewModel : ObservableRecipient
{
    private readonly EnergyUsageModel _model;
    private StemSeries stemSeriesCost;
    private StemSeries stemSeriesEnergy;
    private StemSeries stemSeriesCostHourly;
    private StemSeries stemSeriesEnergyHourly;
    private string localizedEnergy;
    private string localizedCost;
    private string localizedCurrency;
    private string localizedUnit;
    private string localizedDate;
    private string localizedCostLimitLineText;
    private string localizedEnergyLimitLineText;


    [ObservableProperty]
    private string currentMode;

    [ObservableProperty]
    private string currentTimeSpan;

    private bool initialisedHourlyModels = false;

    [ObservableProperty] 
    public PlotModel model;

    [ObservableProperty]
    public PlotModel modelMonitor;

    [ObservableProperty]
    public PlotController controller;

    

    /// <summary>
    /// Initializes a new instance of the <see cref="EnergyUsageViewModel"/> class.
    /// Retrieves localized strings for dynamic content in the viewmodel and initializes the plotmodel and stem series.
    /// </summary>
    /// <param name="model"></param>
    public EnergyUsageViewModel(EnergyUsageModel model)
    {
        _model = model;
        currentMode = "Cost";
        currentTimeSpan = "Daily";
        // initialize localized string versions with UK defaults
        localizedEnergy = "Energy"; localizedCost = "Cost"; 
        localizedCurrency = "£"; localizedUnit = "kWh"; 
        localizedDate = "Date";
        localizedCostLimitLineText = "Daily Cost Limit";
        localizedEnergyLimitLineText = "Daily Energy Limit";

        // initialize plotmodel and stem series in constructor
        this.model = new PlotModel();
        
        this.modelMonitor = new PlotModel();
        
        stemSeriesCost = new StemSeries();
        stemSeriesEnergy = new StemSeries();
        stemSeriesCostHourly = new StemSeries();
        stemSeriesEnergyHourly = new StemSeries();
        this.controller = new PlotController();

        var command = new DelegatePlotCommand<OxyMouseEventArgs>(
           (v, c, a) =>
           {
               Console.WriteLine("1223");
           });
        Controller.BindMouseDown(OxyMouseButton.Left, command); //

        LoadLocalizedStrings();
        InitialiseEnergyUsageModel();
        InitialiseSystemMonitorModel();

    }
    
    private void InitialiseSystemMonitorModel()
    {
        ModelMonitor = new PlotModel
        {
            PlotAreaBorderThickness = new OxyThickness(0),
            DefaultFont = "Segoe UI",
        };

        //generate a random percentage distribution between the 5
        //cake-types (see axis below)
        var rand = new Random();
        double[] powerUsage = new double[5]
        {
            5.00,
            20.00,
            40.00,
            20.00,
            15.00
        };
        var sum = powerUsage.Sum();

        var barSeries = new BarSeries
        {
            ItemsSource = new List<BarItem>(new[]
                {
                new BarItem{ Value = (powerUsage[0] / sum * 100) },
                new BarItem{ Value = (powerUsage[1] / sum * 100) },
                new BarItem{ Value = (powerUsage[2] / sum * 100) },
                new BarItem{ Value = (powerUsage[3] / sum * 100) },
                new BarItem{ Value = (powerUsage[4] / sum * 100) }
        }),
            LabelPlacement = LabelPlacement.Inside,
            LabelFormatString = "{0:.00}%"
        };
        ModelMonitor.Series.Add(barSeries);

        ModelMonitor.Axes.Add(new CategoryAxis
        {
            Position = AxisPosition.Left,
            Key = "CakeAxis",
            ItemsSource = new[]
                {
                "File Explorer",
                "Steam",
                "Chrome",
                "Spotify",
                "Word"
        }
        });
        ModelMonitor.InvalidatePlot(true); // call invalidate plot to update the graph


    }

    /// <summary>
    /// Attempts to load localized strings required for the page.
    /// Uses default UK strings as fallback if unable to load.
    /// </summary>
    private void LoadLocalizedStrings()
    {
        try
        {
            localizedEnergy = "EnergyUsageString_Energy".GetLocalized();
            localizedCost = "EnergyUsageString_Cost".GetLocalized();
            localizedCurrency = "EnergyUsage_CurrencySymbol".GetLocalized();
            localizedUnit = "EnergyUsage_Unit".GetLocalized();
            localizedDate = "EnergyUsageString_Date".GetLocalized();
            localizedCostLimitLineText = "EnergyUsageString_CostLimitLineText".GetLocalized();
            localizedEnergyLimitLineText = "EnergyUsageString_EnergyLimitLineText".GetLocalized();
        }
        catch (Exception e) // use default values if unable to load localized strings
        {
            Debug.WriteLine("Exception: "+e.Message);
        }
    }

    /// <summary>
    /// Initializes the plot model and stem series with daily cost and energy data from the model.
    /// Displays the Daily Cost plot by default.
    /// </summary>
    private void InitialiseEnergyUsageModel()
    {
        Model = new PlotModel
        {
            PlotAreaBorderThickness = new OxyThickness(0),
            DefaultFont = "Segoe UI",
        };

        stemSeriesEnergy = new StemSeries
        {

            TrackerFormatString = $"{localizedDate}: {{2:yyyy-MM-dd}}: {{4:0.0}} {localizedUnit}",
            MarkerStroke = OxyColors.AliceBlue,
            MarkerType = MarkerType.Circle,
        };
        
        

        stemSeriesCost = new StemSeries
        {
            TrackerFormatString = $"{localizedDate}: {{2:yyyy-MM-dd}}: {localizedCurrency}{{4:0.00}}",
            MarkerStroke = OxyColors.AliceBlue,
            MarkerType = MarkerType.Circle,
        };

        var logs = _model.GetDailyEnergyUsageLogs();
        foreach (var log in logs)
        {
            stemSeriesEnergy.Points.Add(new DataPoint(DateTimeAxis.ToDouble(log.Date.Date), log.PowerUsed));
            stemSeriesCost.Points.Add(new DataPoint(DateTimeAxis.ToDouble(log.Date.Date), log.Cost));
        }
        CostModel();

    }


    /// <summary>
    /// Assigns the daily cost stem series to the plot model and performs required plot setup.
    /// </summary>
    [RelayCommand]
    public void CostModel()
    {
        Model.Axes.Clear();
        Model.Series.Clear();
        Model.Annotations.Clear();

        var xAxis = new DateTimeAxis { Position = AxisPosition.Bottom, Title = localizedDate, AxisTitleDistance=12, MinorIntervalType=DateTimeIntervalType.Days};
        var yAxis = new LinearAxis { Position = AxisPosition.Left, Title = localizedCost, Unit = localizedCurrency, ExtraGridlines = new[] { 0.00 },
            IsPanEnabled = false, IsZoomEnabled = false, Minimum=0, AxisTitleDistance=12 };
        Model.Series.Add(stemSeriesCost);
        Model.Axes.Add(yAxis);
        Model.Axes.Add(xAxis);
        AddLimitLine(Model, true);
        Model.InvalidatePlot(true); // call invalidate plot to update the graph
    }

    /// <summary>
    /// Assigns the daily energy stem series to the plot model and performs required setup.
    /// </summary>
    public void EnergyModel()
    {
        Model.Axes.Clear();
        Model.Series.Clear();
        Model.Annotations.Clear();

        var xAxis = new DateTimeAxis { Position = AxisPosition.Bottom, Title = localizedDate, AxisTitleDistance=12, MinorIntervalType=DateTimeIntervalType.Days};
        var yAxis = new LinearAxis
        {
            Position = AxisPosition.Left,
            Title = localizedEnergy,
            Unit = localizedUnit,
            ExtraGridlines = new[] { 0.0 },
            IsPanEnabled = false,
            IsZoomEnabled = false,
            Minimum=0,
            AxisTitleDistance=12
        };
        Model.Series.Add(stemSeriesEnergy);
        Model.Axes.Add(yAxis);
        Model.Axes.Add(xAxis);
        AddLimitLine(Model, false);
        Model.InvalidatePlot(true);
    }


    /// <summary>
    /// Method to add a horizontal line to the plot model to represent the daily cost or energy limit.
    /// </summary>
    /// <param name="plotModel"></param>
    /// <param name="cost">Boolean representing whether to add the limit line for cost (true) or energy (false).</param>
    private void AddLimitLine(PlotModel plotModel, bool cost)
    {
        plotModel.Annotations.Clear();
        var annotation = new LineAnnotation
        {
            Color = OxyColors.AliceBlue,
            LineStyle = LineStyle.Dash,
            Type = LineAnnotationType.Horizontal,
        };
        if (cost)
        {
            annotation.Y = _model.GetDailyCostBudget();
            annotation.Text = localizedCostLimitLineText;
        }
        else
        {
            annotation.Y = _model.GetDailyEnergyBudget();
            annotation.Text = localizedEnergyLimitLineText;
        }
        plotModel.Annotations.Add(annotation);
    }

    /// <summary>
    /// Method called when the user switches to the hourly mode for the graph.
    /// </summary>
    private void HourlyLogs()
    {
        if (!initialisedHourlyModels)
        {
            InitialiseHourlyModels();
        }
        if (CurrentMode.Equals("Cost"))
        {
            CostModelHourly();
        }
        else
        {
            EnergyModelHourly();
        }
    }


    /// <summary>
    /// Command called by the view when the user switches between seeing daily and hourly logs.
    /// </summary>
    [RelayCommand]
    public void SwitchTimeSpan()
    {
        if (CurrentTimeSpan.Equals("Daily"))
        {
            CurrentTimeSpan = "Hourly";
            HourlyLogs();
        }
        else
        {
            CurrentTimeSpan = "Daily";
            if (CurrentMode.Equals("Cost"))
            {
                CostModel();
            }
            else
            {
                EnergyModel();
            }
        }
    }

    /// <summary>
    /// Command called by the view when the user switches between seeing cost and energy data in the graphs.
    /// </summary>
    [RelayCommand]
    public void SwitchCostEnergyModel()
    {
        if (CurrentMode.Equals("Cost"))
        {
            CurrentMode = "Energy";
            if (CurrentTimeSpan.Equals("Daily"))
            {
                EnergyModel();
            }
            else
            {
                EnergyModelHourly();
            }
        }
        else
        {
            CurrentMode = "Cost";
            if (CurrentTimeSpan.Equals("Daily"))
            {
                CostModel();
            }
            else
            {
                CostModelHourly();
            }
        }
    }


    /// <summary>
    /// Initialises hourly stem series for energy and cost when the user first switches to hourly mode.
    /// </summary>
    private void InitialiseHourlyModels()
    {
        initialisedHourlyModels = true;
        stemSeriesEnergyHourly = new StemSeries
        {

            TrackerFormatString = $"{localizedDate}: {{2:yyyy-MM-dd}}: {{4:0.0}} {localizedUnit}",
            MarkerStroke = OxyColors.AliceBlue,
            MarkerType = MarkerType.Circle,
        };

        stemSeriesCostHourly = new StemSeries
        {
            TrackerFormatString = $"{localizedDate}: {{2:yyyy-MM-dd}}: {localizedCurrency}{{4:0.00}}",
            MarkerStroke = OxyColors.AliceBlue,
            MarkerType = MarkerType.Circle,
        };
    }

    /// <summary>
    /// Called when the user switches to the hourly mode and the graph is displaying cost data.
    /// Assigns the hourly cost stem series to the plot model and performs required setup.
    /// </summary>
    private void CostModelHourly()
    {
        Model.Axes.Clear();
        Model.Series.Clear();
        Model.Annotations.Clear();

        var xAxis = new DateTimeAxis { Position = AxisPosition.Bottom, Title = localizedDate, AxisTitleDistance=12 };
        var yAxis = new LinearAxis { Position = AxisPosition.Left, Title = localizedCost, Unit = localizedCurrency, ExtraGridlines = new[] { 0.00 },
            IsPanEnabled = false, IsZoomEnabled = false, Minimum=0, AxisTitleDistance=12};
        Model.Series.Add(stemSeriesCostHourly);
        Model.Axes.Add(yAxis);
        Model.Axes.Add(xAxis);
        Model.InvalidatePlot(true);
    }

    /// <summary>
    /// Called when the user switches to the hourly mode and the graph is displaying energy data.
    /// Assigns the hourly energy stem series to the plot model and performs required setup.
    /// </summary>
    private void EnergyModelHourly()
    {
        Model.Axes.Clear();
        Model.Series.Clear();
        Model.Annotations.Clear();

        var xAxis = new DateTimeAxis { Position = AxisPosition.Bottom, Title = localizedDate };
        var yAxis = new LinearAxis
        {
            Position = AxisPosition.Left,
            Title = localizedEnergy,
            Unit = localizedUnit,
            ExtraGridlines = new[] { 0.0 },
            IsPanEnabled = false,
            IsZoomEnabled = false,
            Minimum=0, 
            AxisTitleDistance=12
        };
        Model.Series.Add(stemSeriesEnergyHourly);
        Model.Axes.Add(yAxis);
        Model.Axes.Add(xAxis);
        Model.InvalidatePlot(true);
    }

}
