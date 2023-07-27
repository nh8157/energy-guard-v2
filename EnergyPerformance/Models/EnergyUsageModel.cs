using System.Data.SQLite;
using System.Diagnostics;
using System.Globalization;
using EnergyPerformance.Contracts.Services;
using EnergyPerformance.Core.Helpers;
using EnergyPerformance.Helpers;
using EnergyPerformance.Services;

namespace EnergyPerformance.Models;


/// <summary> 
/// This class is responsible for storing the data that is used by the EnergyUsageViewModel 
/// </summary>
public class EnergyUsageModel
{
    private readonly EnergyUsageFileService _energyFileService;
    // initialize default values for fallback in case there is no file or the file is corrupted
    private const double DefaultWeeklyBudget = 2.0;
    private const double DefaultCostPerKwh = 0.34;
    private readonly CarbonIntensityInfo _carbonIntensityInfo;

    private EnergyUsageData _energyUsage;
    private readonly IDatabaseService _databaseService;


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

    /// <summary>
    /// The cost per kWh for the user. Returns data loaded from file or default value if file is not found or corrupted.
    /// </summary>

    // We need a way to fetch energy cost live
    public double CostPerKwh
    {
        get
        {
            var cost = DefaultCostPerKwh;
            // additional check incase the file is corrupted or modified
            if (!double.IsNaN(_energyUsage.CostPerKwh) && _energyUsage.CostPerKwh > 0)
            {
                cost = _energyUsage.CostPerKwh;
            }
            else
            {
                _energyUsage.CostPerKwh = cost;
            }
            return cost;
        }
        set
        {
            if (!double.IsNaN(value) && value > 0)
            {
                _energyUsage.CostPerKwh = value;
            }
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
    /// The Accumulated Watts used per app for the current day.
    /// </summary>
    public Dictionary<string, double> AccumulatedWattsPerApp
    {
        get; set;
    }

    /// <summary>
    /// Constructor for the EnergyUsageModel, basic initialization is performed here.
    /// Full initialization is performed in the InitializeAsync method.
    /// </summary>
    /// <param name="fileService"></param>
    public EnergyUsageModel(EnergyUsageFileService fileService, CarbonIntensityInfo carbonIntensityInfo, IDatabaseService databaseService)
    {
        CurrentDay = DateTimeOffset.Now;
        CurrentHour = DateTimeOffset.Now;
        AccumulatedWatts = 0;
        AccumulatedWattsHourly = 0;
        AccumulatedWattsPerApp = new Dictionary<string, double>();
        _energyFileService = fileService;
        _energyUsage = new EnergyUsageData();
        _carbonIntensityInfo = carbonIntensityInfo;
        _databaseService = databaseService;
    }


    /// <summary>
    /// Performs asynchronous tasks required at initialization, including loading data from files. 
    /// Called by the ActivationService.
    /// </summary>
    public async Task InitializeAsync()
    {
        // Initialize energyFileService
        await _databaseService.InitializeDB();
        _energyUsage = await _databaseService.LoadUsageData();
        var current = DateTime.Now;
        if (_energyUsage.Diaries.Count > 0 && _energyUsage.Diaries.Last().Date.Date == current.Date)
        {
            var lastDiary = _energyUsage.Diaries.Last();
            AccumulatedWatts = ConvertKwhToWs(lastDiary.DailyUsage.PowerUsed);
            if (lastDiary.HourlyUsage.Count > 0 && lastDiary.HourlyUsage.Last().Date.Hour == current.Hour)
                AccumulatedWattsHourly = ConvertKwhToWs(lastDiary.HourlyUsage.Last().PowerUsed);
            else
                AccumulatedWattsHourly = 0;
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
        await _databaseService.SaveEnergyData(_energyUsage);
    }

    /// <summary>
    /// Calculates the total cost of energy used for all the days in the current week.
    /// </summary>
    /// <returns>Cost of energy for the previous week</returns>
    public float GetCostForCurrentWeek()
    {
        var current = DateTime.Now;
        float cost = 0;
        var start = _energyUsage.Diaries.Count - 1;
        for (var i = start; i >= 0 && i > start - 7; i--)
        {
            var log = _energyUsage.Diaries[i];
            var date = log.Date.Date;
            if (!CheckTwoDatesAreInTheSameWeek(current, date))
                break;
            cost += log.DailyUsage.Cost;
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

        float cost = 0;
        var start = _energyUsage.Diaries.Count - 1;
        for (var i = start; i >= 0 && i > start - 14; i--)
        {
            var log = _energyUsage.Diaries[i];
            var date = log.Date.Date;
            if (CheckTwoDatesAreInTheSameWeek(previousWeek, date))
                cost += log.DailyUsage.Cost;
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
        var dailyLogs = new List<EnergyUsageLog>();

        foreach (var diary in _energyUsage.Diaries)
            dailyLogs.Add(diary.DailyUsage);

        return dailyLogs;
    }

    /// <summary>
    /// Returns the hourly energy usage logs from the model.
    /// </summary>
    public List<EnergyUsageLog> GetHourlyEnergyUsageLogs()
    {
        var hourlyLogs = new List<EnergyUsageLog>();

        foreach (var diary in _energyUsage.Diaries)
            foreach (var log in diary.HourlyUsage)
                hourlyLogs.Add(log);

        return hourlyLogs;
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

    public double GetEnergyUsed(string proc)
    {
        var energyUsed = ConvertWsToKwh(AccumulatedWattsPerApp.GetValueOrDefault(proc, 0));
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
    /// Calculates the daily cost of a process
    /// </summary>
    /// <param name="proc">Name of the process</param>
    private double GetDailyCost(string proc)
    {
        return GetEnergyUsed(proc) * CostPerKwh;
    }

    /// <summary>
    /// Calculates the cost of energy used in the last hour.
    /// </summary>
    private double GetHourlyCost()
    {
        return GetEnergyUsedHourly() * CostPerKwh;
    }

    /// <summary>
    /// Calculates the daily carbon emission of the machine
    /// </summary>
    private double GetDailyCarbonEmission()
    {
        return GetEnergyUsed() * CarbonIntensity;
    }

    /// <summary>
    /// Calculates the daily carbon emission of a process
    /// </summary>
    /// <param name="proc">Name of the process</param>
    private double GetDailyCarbonEmission(string proc)
    {
        return GetEnergyUsed(proc) * CarbonIntensity;
    }

    /// <summary>
    /// Calculates the hourly carbon emission of the machine
    /// </summary>
    private double GetHourlyCarbonEmission()
    {
        return GetEnergyUsedHourly() * CarbonIntensity;
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
    /// Creates a new EnergyUsageLog for the total daily measurement, the hourly measurement, as well as per process measurement
    /// then adds this to EnergyUsageData which stores all records.
    /// </summary>
    public void Update()
    {
        var current = DateTime.Now;
        var energyUsedCurr = GetEnergyUsed();
        var energyUsedHourlyCurr = GetEnergyUsedHourly();

        // Update daily log
        if (!(_energyUsage.Diaries.Count > 0) || _energyUsage.Diaries.Last().Date.Date < current.Date)
            _energyUsage.Diaries.Add(new EnergyUsageDiary());

        var lastDiary = _energyUsage.Diaries.Last();

        var lastDailyUsage = lastDiary.DailyUsage;
        var energyUsedPrev = lastDailyUsage.PowerUsed;
        lastDailyUsage.CarbonEmission += (float)((energyUsedCurr - energyUsedPrev) * CarbonIntensity);
        lastDailyUsage.Cost += (float)((energyUsedCurr - energyUsedPrev) * CostPerKwh);
        lastDailyUsage.PowerUsed = (float)energyUsedCurr;

        // Update hourly log
        if (!(lastDiary.HourlyUsage.Count > 0) || lastDiary.HourlyUsage.Last().Date.Hour < current.Hour)
            lastDiary.HourlyUsage.Add(new EnergyUsageLog());

        var lastHourlyUsage = lastDiary.HourlyUsage.Last();
        var energyUsedHourlyPrev = lastHourlyUsage.PowerUsed;
        lastHourlyUsage.CarbonEmission += (float)((energyUsedHourlyCurr - energyUsedHourlyPrev) * CarbonIntensity);
        lastHourlyUsage.Cost += (float)((energyUsedHourlyCurr - energyUsedHourlyPrev) * CostPerKwh);
        lastHourlyUsage.PowerUsed = (float)energyUsedHourlyCurr;
        
        // Update per process usage
        foreach (var proc in AccumulatedWattsPerApp.Keys)
            lastDiary.PerProcUsage[proc] = new EnergyUsageLog(current, (float)GetEnergyUsed(proc), (float)GetDailyCost(proc), (float)GetDailyCarbonEmission(proc));

        Debug.WriteLine($"Recorded daily usage: {lastDiary.DailyUsage.PowerUsed}");
        Debug.WriteLine($"Recorded hourly usage: {lastDiary.HourlyUsage.Last().PowerUsed}");
        Debug.WriteLine(lastDiary.DailyUsage.PowerUsed - lastDiary.HourlyUsage.Last().PowerUsed);
        Debug.WriteLine("Model updated.");

    }

}