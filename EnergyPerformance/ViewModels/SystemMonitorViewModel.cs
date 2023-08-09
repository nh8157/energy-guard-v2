using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EnergyPerformance.Models;
using OxyPlot.Annotations;
using OxyPlot.Axes;
using OxyPlot.Series;
using OxyPlot;
using CLI;
using System.Diagnostics;

namespace EnergyPerformance.ViewModels;
public partial class SystemMonitorViewModel : ObservableRecipient
{
    //private readonly EnergyUsageModel _model;


    [ObservableProperty]
    public PlotModel model;

    public PlotController controller;

    /// <summary>
    /// Initializes a new instance of the <see cref="EnergyUsageViewModel"/> class.
    /// Retrieves localized strings for dynamic content in the viewmodel and initializes the plotmodel and stem series.
    /// </summary>
    /// <param name="model"></param>
    //public EnergyUsageViewModel(EnergyUsageModel model)
    public SystemMonitorViewModel()
    {
        //_model = model
        // initialize plotmodel and stem series in constructor
        this.model = new PlotModel();
        InitialiseEnergyUsageModel();
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

        //generate a random percentage distribution between the 5
        //cake-types (see axis below)
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
        Model.Series.Add(barSeries);

        Model.Axes.Add(new CategoryAxis
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
        Model.InvalidatePlot(true); // call invalidate plot to update the graph


    }




}
