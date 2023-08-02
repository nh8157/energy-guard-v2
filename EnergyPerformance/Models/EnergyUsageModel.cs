using System.Diagnostics;
using System.Globalization;
using EnergyPerformance.Core.Helpers;
using EnergyPerformance.Services;
using Newtonsoft.Json;

namespace EnergyPerformance.Models;


/// <summary> 
/// This class is responsible for storing the data that is used by the EnergyUsageViewModel 
/// </summary>
public class EnergyUsageModel
{
    // initialize default values for fallback in case there is no file or the file is corrupted
    private readonly double DefaultWeeklyBudget = 2.0;
    private double DefaultCostPerKwh = 0.34;

    private readonly CarbonIntensityInfo _carbonIntensityInfo;
    private readonly EnergyRateInfo _energyRateInfo;

    private EnergyUsageData _energyUsage;

    public bool IsLiveCost
    {
        get; set;
    }

    public DateTimeOffset CurrentDay
    {
        get; set;
    }
    public DateTimeOffset CurrentHour
    {
        get; set;
    }

    public double CarbonIntensity
    {
        get => _carbonIntensityInfo.CarbonIntensity;
        private set => _carbonIntensityInfo.CarbonIntensity = value;
    }

    public double LiveCostPerKwh => _energyRateInfo.EnergyRate;

    /// <summary>
    /// The cost per kWh for the user. Returns data loaded from file or default value if file is not found or corrupted.
    /// </summary>

    public double CostPerKwh
    {
        get
        {
            var cost = DefaultCostPerKwh;
            // additional check incase the file is corrupted or modified
            if (IsLiveCost && LiveCostPerKwh > 0)
                cost = LiveCostPerKwh;

            return cost;
        }
        set
        {
            if (!IsLiveCost && !double.IsNaN(value) && value > 0)
                DefaultCostPerKwh = value;
        }
    }

    /// <summary>
    /// The weekly budget for the user in kWh. Returns data loaded from file or default value if file is not found or corrupted.
    /// </summary>
    public double WeeklyBudget
    {
        get
        {
            var budget = DefaultWeeklyBudget;
            // additional check incase the file is corrupted or modified
            if (!double.IsNaN(_energyUsage.WeeklyBudget) && _energyUsage.WeeklyBudget > 0)
            {
                budget = _energyUsage.WeeklyBudget;
            }
            else
            {
                _energyUsage.WeeklyBudget = budget;
            }
            return budget;
        }
        set
        {
            if (!double.IsNaN(value) && value > 0)
            {
                _energyUsage.WeeklyBudget = value;
            }
        }
    }

    /// <summary>
    /// The Accumulated Watts used for the current day.
    /// </summary>
    public double AccumulatedWatts
    {
        get; set;
    }

    /// <summary>
    /// The Accumulated Watts used for the current hour.
    /// </summary>
    public double AccumulatedWattsHourly
    {
        get; set;
    }

    /// <summary>
    /// Constructor for the EnergyUsageModel, basic initialization is performed here.
    /// Full initialization is performed in the InitializeAsync method.
    /// </summary>
    /// <param name="fileService"></param>
    public EnergyUsageModel(EnergyUsageFileService fileService, CarbonIntensityInfo carbonIntensityInfo, LocationInfo locationInfo, EnergyRateInfo energyRateInfo, IDatabaseService databaseService)
    {
        CurrentDay = DateTimeOffset.Now;
        CurrentHour = DateTimeOffset.Now;
        IsLiveCost = true;
        AccumulatedWatts = 0;
        AccumulatedWattsHourly = 0;
        AccumulatedWattsPerApp = new Dictionary<string, double>();
        _energyUsage = new EnergyUsageData();
        _carbonIntensityInfo = carbonIntensityInfo;
        _energyRateInfo = energyRateInfo;
        _databaseService = databaseService;
    }


    /// <summary>
    /// Performs asynchronous tasks required at initialization, including loading data from files. 
    /// Called by the ActivationService.
    /// </summary>
    public async Task InitializeAsync()
    {
        // Initialize energyFileService
        _energyUsage = await _energyFileService.ReadFileAsync();
        var lastMeasurement = _energyUsage.LastMeasurement;
        var current = DateTime.Now;
        if (_energyUsage.LastMeasurement.Date.Date == current.Date)
        {
            AccumulatedWatts = ConvertKwhToWs(lastMeasurement.PowerUsed);
        }
        else if (LoadLastHourlyLog(current))
        {
            AccumulatedWattsHourly = ConvertKwhToWs(_energyUsage.HourlyLogs.Last().PowerUsed);
        }
        else
        {
            AccumulatedWatts = 0;
            AccumulatedWattsHourly = 0;
        }
    }

    /// <summary>
    /// Saves the current state of the model to the data file in local app data storage through the EnergyUsageFileService.
    /// </summary>
    public async Task Save()
    {
        Update(); // update the model before saving.
        Debug.WriteLine("Saving model.");
        await _energyFileService.SaveFileAsync();
    }


    /// <summary>
    /// Checks if the last saved hourly log is from the current hour.
    /// </summary>
    /// <param name="current">Current date and time</param>
    /// <returns>True if the last saved hourly log is from the current hour, false otherwise</returns>
    private bool LoadLastHourlyLog(DateTime current)
    {
        if (_energyUsage.HourlyLogs.Count > 0)
        {
            if (_energyUsage.HourlyLogs.Last().Date.Hour == current.Hour)
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Calculates the total cost of energy used for all the days in the current week.
    /// </summary>
    /// <returns>Cost of energy for the previous week</returns>
    public float GetCostForCurrentWeek()
    {
        var current = DateTime.Now;
        float cost = 0;
        var start = _energyUsage.DailyLogs.Count - 1;
        for (var i = start; i >= 0 && i > start - 7; i--)
        {
            var log = _energyUsage.DailyLogs[i];
            var date = log.Date.Date;
            if (!CheckTwoDatesAreInTheSameWeek(current, date))
            {
                break;
            }
            cost += log.Cost;
        }
        return cost;
    }

    /// <summary>
    /// Calculates the total cost of energy used for all the days in the previous week.
    /// </summary>
    /// <returns>Cost of energy for the previous week</returns>
    public float GetCostForPreviousWeek()
    {
        // iterate through _energyUsage.DailyLogs backwards and calculate the cost for all the days in the previous week
        var current = DateTime.Now;
        var previousWeek = current.AddDays(-7);
        var previousFortnight = current.AddDays(-14);

        float cost = 0;
        var start = _energyUsage.DailyLogs.Count - 1;
        for (var i = start; i >= 0 && i > start - 14; i--)
        {
            var log = _energyUsage.DailyLogs[i];
            var date = log.Date.Date;
            if (CheckTwoDatesAreInTheSameWeek(previousWeek, date))
            {
                cost += log.Cost;
            }
            else if (CheckTwoDatesAreInTheSameWeek(previousFortnight, date))
            {
                break;
            }
        }
        return cost;
    }


    /// <summary>
    /// Checks if two dates are in the same week.
    /// </summary>
    /// <param name="date1">The first date.</param>
    /// <param name="date2">The second date.</param>
    /// <returns>True if the two dates are in the same week, false otherwise.</returns>
    private bool CheckTwoDatesAreInTheSameWeek(DateTime date1, DateTime date2)
    {
        // Calculates the starting day of the week for both given dates and checks if they are the same.
        // If true, both dates are in the same week.

        // Note: DayOfWeek's built-in enum starts from Sunday so subtract 1 at the end
        var cal = DateTimeFormatInfo.CurrentInfo.Calendar;
        var startDayOfWeekDate1 = date1.Date.AddDays(-1 * (int)cal.GetDayOfWeek(date1) - 1);
        var startDayOfWeekDate2 = date2.Date.AddDays(-1 * (int)cal.GetDayOfWeek(date2) - 1);
        return startDayOfWeekDate1 == startDayOfWeekDate2;
    }


    /// <summary>
    /// Returns the daily energy usage logs from the model.
    /// </summary>
    public List<EnergyUsageLog> GetDailyEnergyUsageLogs()
    {
        return _energyUsage.DailyLogs;

    }

    /// <summary>
    /// Returns the hourly energy usage logs from the model.
    /// </summary>
    public List<EnergyUsageLog> GetHourlyEnergyUsageLogs()
    {
        return _energyUsage.HourlyLogs;
    }

    /// <summary>
    /// Calculates the energy used in the last day.
    /// </summary>
    public double GetEnergyUsed()
    {
        /// 1s is the sampling rate, divide by 3.6Mil to convert Ws -> kWh
        var energyUsed = ConvertWsToKwh(AccumulatedWatts);
        return energyUsed;

    }

    /// <summary>
    /// Calculates the energy used in the last hour.
    /// </summary>
    private double GetEnergyUsedHourly()
    {
        // 1s is the sampling rate, divide by 3.6Mil to convert Ws -> kWh
        var energyUsed = ConvertWsToKwh(AccumulatedWattsHourly);
        return energyUsed;

    }

    /// <summary>
    /// Converts energy usage value from Ws to kWh.
    /// </summary>
    /// <param name="value">Energy usage value in Ws.</param>
    private double ConvertWsToKwh(double value)
    {
        return (value * 1) / 3600000;
    }


    /// <summary>
    /// Converts energy usage value from kWh to Ws.
    /// </summary>
    /// <param name="value">Energy usage value in kWh.</param>
    private double ConvertKwhToWs(double value)
    {
        return value * 3600000;
    }

    /// <summary>
    /// Calculates the cost of energy used in the last day.
    /// </summary>
    private double GetDailyCost()
    {
        return GetEnergyUsed() * CostPerKwh;

    }


    /// <summary>
    /// Calculates the cost of energy used in the last hour.
    /// </summary>
    private double GetHourlyCost()
    {
        return GetEnergyUsedHourly() * CostPerKwh;

    }


    /// <summary>
    /// Calculates a daily cost budget based on the weekly budget.
    /// Used for displaying budget line on EnergyUsage page.
    /// </summary>
    public double GetDailyCostBudget()
    {
        return Math.Round(WeeklyBudget / 7.0, 2);
    }


    /// <summary>
    /// Calculates a daily energy budget based on the weekly budget and cost per unit.
    /// </summary>
    public double GetDailyEnergyBudget()
    {
        // Calculate weekly energy budget based on cost per unit and weekly budget and divide by 7 to determine daily budget
        var res = WeeklyBudget / CostPerKwh / 7.0;
        return Math.Round(res, 2);
    }


    /// <summary>
    /// Update the model with the latest energy usage data.
    /// Creates a new EnergyUsageLog for the total daily measurement as well as the hourly measurement and
    /// adds this to EnergyUsageData which stores all records.
    /// </summary>
    public void Update()
    {
        var current = DateTime.Now;
        var lastMeasurement = new EnergyUsageLog(current, (float)GetEnergyUsed(), (float)GetDailyCost(), (float)GetDailyCarbonEmission());
        var lastMeasurementHourly = new EnergyUsageLog(current, (float)GetEnergyUsedHourly(), (float)GetHourlyCost(), (float)GetHourlyCarbonEmission());
        Debug.WriteLine($"Current energy cost: {CostPerKwh}");

        // Update daily log
        if (!(_energyUsage.Diaries.Count > 0) || _energyUsage.Diaries.Last().Date.Date < current.Date)
            _energyUsage.Diaries.Add(new EnergyUsageDiary());

        var lastDiary = _energyUsage.Diaries.Last();
        lastDiary.DailyUsage = lastMeasurement;

        // update daily logs
        if (!(_energyUsage.DailyLogs.Count > 0))
        {
            _energyUsage.DailyLogs.Add(lastMeasurement);
        }
        else
        {
            if (_energyUsage.DailyLogs.Last().Date.Date == current.Date)
            {
                // change the last element of _energyUsage.DailyLogs to equal lastMeasurement
                _energyUsage.DailyLogs[^1] = lastMeasurement;
            }
            else if (_energyUsage.DailyLogs.Last().Date.Date.CompareTo(current.Date) < 0)  // If the current date is later than the last recorded date, then add a new entry to the list
            {
                _energyUsage.DailyLogs.Add(lastMeasurement);
            }
        }

        // update hourly logs
        if (!(_energyUsage.HourlyLogs.Count > 0))
        {
            _energyUsage.HourlyLogs.Add(lastMeasurementHourly);
        }
        else
        {
            if (_energyUsage.HourlyLogs.Last().Date.Hour == current.Hour)
            {
                _energyUsage.HourlyLogs[^1] = lastMeasurementHourly;
            }
            else if (_energyUsage.HourlyLogs.Last().Date.Date.CompareTo(current.Date) < 0)  // If the current time is later than the last recorded tune, then add a new entry to the list
            {
                _energyUsage.HourlyLogs.Add(lastMeasurementHourly);
            }
        }

        Debug.WriteLine("Model updated.");

    }

}