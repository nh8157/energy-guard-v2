
namespace EnergyPerformance.Core.Helpers;

/// <summary>
/// Class to represent the energy usage data tracked over time for the application.
/// EnergyUsageModel contains a reference to an instance of this class which is used to store/retrieve all energy usage data.
/// </summary>
public class EnergyUsageData
{
    public double CostPerKwh
    {
        get;
        set;
    }

    public double WeeklyBudget
    {
        get; set;
    }

    public EnergyUsageLog LastMeasurement
    {
        get; set;
    }


    public List<EnergyUsageLog> HourlyLogs
    {
        get; set;
    }


    public List<EnergyUsageLog> DailyLogs
    {
        get; set;
    }


    public EnergyUsageData(double costPerKwh, double weeklyBudget, EnergyUsageLog lastMeasurement, List<EnergyUsageLog> hourlyLogs, List<EnergyUsageLog> dailyLogs)
    {
        CostPerKwh = costPerKwh;
        WeeklyBudget = weeklyBudget;
        LastMeasurement = lastMeasurement;
        HourlyLogs = hourlyLogs;
        DailyLogs = dailyLogs;
    }

    public EnergyUsageData()
    {
        CostPerKwh = 0;
        WeeklyBudget = 0;
        LastMeasurement = new EnergyUsageLog(DateTime.Now, 0, 0);
        HourlyLogs = new List<EnergyUsageLog>();
        DailyLogs = new List<EnergyUsageLog>();
    }
}
