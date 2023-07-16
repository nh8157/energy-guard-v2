using System.Data.SQLite;
using EnergyPerformance.Core.Helpers;

namespace EnergyPerformance.Contracts.Services;
public interface IDatabaseService
{
    public void InitializeDB();

    public Task<string> InsertDailyLog(EnergyUsageLog data);

    public Task InsertHourlyLog(int hour, EnergyUsageLog data);

    public Task InsertProgramLog(string programID, EnergyUsageLog data);

    public Task InsertEnergyDiary(EnergyUsageDiary Diary);

    public EnergyUsageLog RetrieveDailyLogByDate(string date);

    public List<EnergyUsageLog> RetrieveHourlyLogByDate(string date);

    public Dictionary<string, EnergyUsageLog> RetrieveProgramLogByDate(string date);

    public EnergyUsageDiary RetrieveDiaryByDate(string date);

    public void RetrieveAllDiaries();
}
