using System.Data.SQLite;
using EnergyPerformance.Core.Helpers;

namespace EnergyPerformance.Contracts.Services;
public interface IDatabaseService
{
    public SQLiteConnection CreateConnection();

    public void InitializeDB();
    public string InsertNewLog(EnergyUsageLog data, string type);

    public EnergyUsageLog ReadFromLogByID(string id);

    public void InsertHourlyLog(int hour, EnergyUsageLog data);

    public List<EnergyUsageLog> ReadFromHourlyLogByDay(string date);

    public void RetrieveAllDiaries();
}
