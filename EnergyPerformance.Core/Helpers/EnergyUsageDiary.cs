namespace EnergyPerformance.Core.Helpers;
public class EnergyUsageDiary
{
    public DateTime Date;
    /// <summary>
    /// This property logs the total energy usage/carbon emission/cost of the day
    /// </summary>
    public EnergyUsageLog DailyUsage
    {
        get; set;
    }

    /// <summary>
    /// This property is a breakdown of each hour's energy usage arranged in a list
    /// </summary>
    public List<EnergyUsageLog> HourlyUsage
    {
        get; set;
    }

    /// <summary>
    /// Energy consumption per applicationon the day
    /// </summary>
    public Dictionary<string, EnergyUsageLog> PerProcUsage
    {
        get; set;
    }

    public EnergyUsageDiary(DateTime date, EnergyUsageLog dailyLog, List<EnergyUsageLog> hourlyLog, Dictionary<string, EnergyUsageLog> perProcLog)
    {
        Date = date;
        DailyUsage = dailyLog;
        HourlyUsage = hourlyLog;
        PerProcUsage = perProcLog;
    }

    public EnergyUsageDiary()
    {
        Date = DateTime.Now;
        DailyUsage = new EnergyUsageLog(Date, 0, 0, 0);
        HourlyUsage = new List<EnergyUsageLog>();
        PerProcUsage = new Dictionary<string, EnergyUsageLog>();
    }
}
