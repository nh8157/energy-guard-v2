using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Globalization;
using EnergyPerformance.Contracts.Services;
using EnergyPerformance.Core.Helpers;
using EnergyPerformance.Models;

namespace EnergyPerformance.Services;
public class DatabaseService : IDatabaseService
{

    private readonly string _localApplicationData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
    private readonly string _datasource;
    private EnergyUsageData _energyUsage;
    public EnergyUsageData EnergyUsag => _energyUsage;

    private bool _isInitialized = false;

    public DatabaseService()
    {
        _datasource = Path.Combine(_localApplicationData, "EnergyPerformance/ApplicationData/database.db");
        _energyUsage = new EnergyUsageData();
    }

    public async Task InitializeDB()
    {
        if (!File.Exists(_datasource))
        {
            try
            {
                SQLiteConnection.CreateFile(_datasource);
                SQLiteConnection conn = CreateConnection();
                SQLiteCommand sqlite_cmd;
                string CreateEnergyUsageLogTable = "CREATE TABLE energy_usage_log (log_id TEXT NOT NULL PRIMARY KEY , " +
                    "date TEXT NOT NULL, exact_date_time TEXT NOT NULL, power_used NUMERIC, cost NUMERIC, " +
                    "carbon_emission NUMERIC, type TEXT NOT NULL)";
                string CreateDiaryTable = "CREATE TABLE energy_diary_log (diary_log_id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT , " +
                    "date TEXT NOT NULL,  exact_date_time TEXT NOT NULL, daily_log_id TEXT, cost_per_kwh NUMERIC, weekly_budget NUMERIC," +
                    " FOREIGN KEY (daily_log_id) REFERENCES energy_usage_log(log_id))";
                string CreateProgramLogTable = "CREATE TABLE program_log (date TEXT NOT NULL, exact_date_time TEXT NOT NULL, " +
                    "program_id TEXT NOT NULL, log_id TEXT NOT NULL PRIMARY KEY, " +
                    "FOREIGN KEY (log_id) REFERENCES energy_usage_log(log_id))";
                string CreateHourlyLogTable = "CREATE TABLE energy_hourly_log (date TEXT NOT NULL, exact_date_time TEXT NOT NULL," +
                    " hour INTEGER NOT NULL, log_id TEXT NOT NULL PRIMARY KEY, " +
                    "FOREIGN KEY (log_id) REFERENCES energy_usage_log(log_id))";
                sqlite_cmd = conn.CreateCommand();
                sqlite_cmd.CommandText = CreateEnergyUsageLogTable;
                sqlite_cmd.ExecuteNonQuery();
                sqlite_cmd = conn.CreateCommand();
                sqlite_cmd.CommandText = CreateDiaryTable;
                sqlite_cmd.ExecuteNonQuery();
                sqlite_cmd = conn.CreateCommand();
                sqlite_cmd.CommandText = CreateProgramLogTable;
                sqlite_cmd.ExecuteNonQuery();
                sqlite_cmd = conn.CreateCommand();
                sqlite_cmd.CommandText = CreateHourlyLogTable;
                sqlite_cmd.ExecuteNonQuery();
                conn.Close();
            }
            catch (Exception ex)
            {
                App.GetService<DebugModel>().AddMessage(ex.ToString());
            }
        }
        if (await DatabaseIsActive())
        {
            _energyUsage = await ReadUsageDataFromDatabase();
            App.GetService<DebugModel>().AddMessage($"retrieved {_energyUsage.Diaries.Count().ToString()} diaries from databased");
        }
        else
        {
            App.GetService<DebugModel>().AddMessage("Empty Database");
        }
        _isInitialized = true;
    }


    private SQLiteConnection CreateConnection()
    {
        SQLiteConnection sqlite_conn;
        sqlite_conn = new SQLiteConnection($"Data Source={_datasource}; Version = 3; New = True; Compress = True; ");
        try
        {
            sqlite_conn.Open();
        }
        catch (Exception ex)
        {
            App.GetService<DebugModel>().AddMessage(ex.ToString());
        }
        return sqlite_conn;
    }

    private async Task<SQLiteConnection> CreateConnectionAsync()
    {
        SQLiteConnection sqlite_conn;
        sqlite_conn = new SQLiteConnection($"Data Source={_datasource}; Version = 3; New = True; Compress = True; ");
        try
        {
            await sqlite_conn.OpenAsync();
        }
        catch (Exception ex)
        {
            App.GetService<DebugModel>().AddMessage(ex.ToString());
        }
        return sqlite_conn;
    }

    private async Task<string> InsertNewLog(SQLiteConnection conn, EnergyUsageLog data, string type)
    {
        try
        {
            SQLiteCommand command = conn.CreateCommand();
            command.CommandText = "INSERT INTO energy_usage_log(log_id, date, exact_date_time, " +
                "power_used, cost, carbon_emission, type) " +
                "VALUES (@log_id, @date, @exact_date_time, @power_usage, @cost, @carbon_emission, @type)";
            string id = EncodeID(data.Date, type);
            command.Parameters.AddWithValue("@log_id", id);
            command.Parameters.AddWithValue("@date", data.Date.ToString("yyyy/MM/dd"));
            command.Parameters.AddWithValue("@exact_date_time", data.Date.ToString("yyyy/MM/dd HH:mm:ss"));
            command.Parameters.AddWithValue("@power_usage", data.PowerUsed);
            command.Parameters.AddWithValue("@cost", data.Cost);
            command.Parameters.AddWithValue("@carbon_emission", data.CarbonEmission);
            command.Parameters.AddWithValue("@type", type);
            await command.ExecuteNonQueryAsync();
            return id;
        }
        catch (Exception ex)
        {
            App.GetService<DebugModel>().AddMessage(ex.ToString());
            return string.Empty;
        }
    }

    private string EncodeID(DateTime date, string type)
    {
        string encodedDate = date.ToString("yyyyMMdd");
        string encodedTime = date.ToString("HHmmss");
        return encodedDate + type + encodedTime;
    }

    public async Task<string> InsertDailyLog(EnergyUsageLog data)
    {
        try
        {
            SQLiteConnection conn = await CreateConnectionAsync();
            string date = data.Date.ToString("yyyy/MM/dd");
            if (await CheckIfDailyLogExists(conn, date))
            {
                await DeleteParentLog(conn, "energy_usage_log", date, "type = \"D\"");
            }
            string id = await InsertNewLog(conn, data, "D");
            conn.Close();
            return id;
        }
        catch (Exception ex)
        {
            App.GetService<DebugModel>().AddMessage(ex.ToString());
            return string.Empty;
        }
    }

    public async Task InsertHourlyLog(int hour, EnergyUsageLog data)
    {
        try
        {
            SQLiteConnection conn = await CreateConnectionAsync();
            SQLiteCommand command = conn.CreateCommand();
            string date = data.Date.ToString("yyyy/MM/dd");
            string exactDate = data.Date.ToString("yyyy/MM/dd HH:mm:ss");
            if (await CheckIfHourlyLogExists(conn, date, hour))
            {
                SQLiteCommand fetchCmd = new SQLiteCommand($"SELECT log_id FROM energy_hourly_log " +
                    $"WHERE date = \"{date}\" AND hour = {hour}", conn);
                string log_id = fetchCmd.ExecuteScalar().ToString();
                await DeleteParentLog(conn, "energy_hourly_log", date, $"hour = {hour}");
                await DeleteUsageLog(conn, log_id);
            }
            SQLiteCommand cmd = new SQLiteCommand("INSERT INTO energy_hourly_log(date, exact_date_time, hour, log_id ) " +
                "VALUES (@date, @exact_date_time, @hour, @id)", conn);
            string id = await InsertNewLog(conn, data, "H");
            cmd.Parameters.AddWithValue("@date", date);
            cmd.Parameters.AddWithValue("@exact_date_time", exactDate);
            cmd.Parameters.AddWithValue("@hour", hour);
            cmd.Parameters.AddWithValue("@id", id);
            await cmd.ExecuteNonQueryAsync();
            conn.Close();
        }
        catch (Exception ex)
        {
            App.GetService<DebugModel>().AddMessage(ex.ToString());
        }
    }


    public async Task InsertProgramLog(string programID, EnergyUsageLog data)
    {
        try
        {
            SQLiteConnection conn = await CreateConnectionAsync();
            SQLiteCommand command = conn.CreateCommand();
            string date = data.Date.ToString("yyyy/MM/dd");
            string exactDate = data.Date.ToString("yyyy/MM/dd HH:mm:ss");
            if (await CheckIfProgramLogExists(conn, date, programID))
            {
                SQLiteCommand fetchCmd = new SQLiteCommand($"SELECT log_id FROM program_log " +
                    $"WHERE date = \"{date}\" AND program_id = \"{programID}\"", conn);
                string log_id = fetchCmd.ExecuteScalar().ToString();
                await DeleteParentLog(conn, "program_log", date, $"program_id = \"{programID}\"");
                await DeleteUsageLog(conn, log_id);
            }
            SQLiteCommand cmd = new SQLiteCommand("INSERT INTO program_log(date, exact_date_time, program_id, log_id ) " +
                "VALUES (@date, @exact_date_time, @programID, @id)", conn);
            string id = await InsertNewLog(conn, data, "P");
            cmd.Parameters.AddWithValue("@date", date);
            cmd.Parameters.AddWithValue("@exact_date_time", exactDate);
            cmd.Parameters.AddWithValue("@programID", programID);
            cmd.Parameters.AddWithValue("@id", id);
            await cmd.ExecuteNonQueryAsync();
            conn.Close();
        }
        catch (Exception ex)
        {
            App.GetService<DebugModel>().AddMessage(ex.ToString());
        }
    }

    public async Task InsertEnergyDiary(EnergyUsageDiary Diary)
    {
        try
        {
            string date = Diary.Date.ToString("yyyy/MM/dd");
            EnergyUsageLog DailyLog = Diary.DailyUsage;
            List<EnergyUsageLog> HourlyLogs = Diary.HourlyUsage;
            Dictionary<string, EnergyUsageLog> ProgramLogs = Diary.PerProcUsage;
            string dailyLogID = await InsertDailyLog(DailyLog);

            foreach (EnergyUsageLog hourlylog in HourlyLogs)
            {
                int hour = hourlylog.Date.Hour;
                await InsertHourlyLog(hour, hourlylog);
            }
            foreach ((string programID, EnergyUsageLog programLog) in ProgramLogs)
            {
                await InsertProgramLog(programID, programLog);
            }
            SQLiteConnection conn = await CreateConnectionAsync();
            if (await CheckIfDiaryExists(conn, date))
            {
                string updateQuery = $"UPDATE energy_diary_log SET daily_log_id = \"{dailyLogID}\" WHERE date = \"{date}\"";
                SQLiteCommand deleteCommand = new SQLiteCommand(updateQuery, conn);
                await deleteCommand.ExecuteNonQueryAsync();
            }
            else
            {
                SQLiteCommand cmd = new SQLiteCommand("INSERT INTO energy_diary_log (date, exact_date_time, daily_log_id ) " +
                    "VALUES (@date, @exact_date_time, @daily_log_id)", conn);
                cmd.Parameters.AddWithValue("@date", date);
                cmd.Parameters.AddWithValue("@exact_date_time", Diary.Date.ToString("yyyy/MM/dd HH:mm:ss"));
                cmd.Parameters.AddWithValue("@daily_log_id", dailyLogID);
                await cmd.ExecuteNonQueryAsync();
            }
            conn.Close();
        }
        catch (Exception ex)
        {
            App.GetService<DebugModel>().AddMessage(ex.ToString());
        }
    }

    private async Task UpdateBudgetAndCostPerKwh(string date, double costPerKwh, double weeklyBudget)
    {
        SQLiteConnection conn = CreateConnection();
        string query = $"UPDATE energy_diary_log SET cost_per_kwh = {costPerKwh}, " +
            $"weekly_budget = {weeklyBudget} WHERE date = \"{date}\"";
        SQLiteCommand comm = new SQLiteCommand(query, conn);
        await comm.ExecuteNonQueryAsync();
        conn.Close();
    }

    public async Task SaveEnergyData(EnergyUsageData data)
    {
        try
        {
            EnergyUsageData currentUsageData = await ReadUsageDataFromDatabase();
            foreach (EnergyUsageDiary diary in data.Diaries)
            {
                if (!(currentUsageData.Diaries.Contains(diary)))
                {
                    await InsertEnergyDiary(diary);
                    string currentDate = diary.Date.ToString("yyyy/MM/dd");
                    await UpdateBudgetAndCostPerKwh(currentDate, data.CostPerKwh, data.WeeklyBudget);
                }
            }
            App.GetService<DebugModel>().AddMessage("Usage Data Saved To DB");
        }
        catch (Exception ex)
        {
            App.GetService<DebugModel>().AddMessage(ex.ToString());
        }
    }


    private EnergyUsageLog ReadFromLogByID(string id)
    {
        SQLiteConnection conn = CreateConnection();
        SQLiteCommand cmd = new SQLiteCommand("SELECT * FROM energy_usage_log WHERE log_id = @id ", conn);
        cmd.Parameters.AddWithValue("@id", id);
        SQLiteDataReader reader = cmd.ExecuteReader();
        EnergyUsageLog EnergyLog = new EnergyUsageLog();
        if (reader.Read())
        {
            string exactDate = reader.GetString(2);
            DateTime time = DateTime.ParseExact(exactDate, "yyyy/MM/dd HH:mm:ss", CultureInfo.InvariantCulture);
            float powerUsed = reader.GetFloat(3);
            float cost = reader.GetFloat(4);
            float carbonEmission = reader.GetFloat(5);
            EnergyLog = new EnergyUsageLog(time, powerUsed, cost, carbonEmission);
        }
        reader.Close();
        conn.Close();
        return EnergyLog;
    }

    public EnergyUsageLog RetrieveDailyLogByDate(string date)
    {
        SQLiteConnection conn = CreateConnection();
        string query = $"SELECT * FROM energy_usage_log WHERE date = \"{date}\" AND type = \"D\"";
        SQLiteCommand cmd = new SQLiteCommand(query, conn);
        SQLiteDataReader reader = cmd.ExecuteReader();
        EnergyUsageLog log = new EnergyUsageLog();
        if (reader.Read())
        {
            try
            {
                string exactDate = reader.GetString(2);
                DateTime time = DateTime.ParseExact(exactDate, "yyyy/MM/dd HH:mm:ss", CultureInfo.InvariantCulture);
                float powerUsed = reader.GetFloat(3);
                float cost = reader.GetFloat(4);
                float carbonEmission = reader.GetFloat(5);
                log = new EnergyUsageLog(time, powerUsed, cost, carbonEmission);
            }
            catch (Exception ex)
            {
                App.GetService<DebugModel>().AddMessage(ex.ToString());
            }
        }
        reader.Close();
        conn.Close();
        return log;
    }

    public List<EnergyUsageLog> RetrieveHourlyLogByDate(string date)
    {
        SQLiteConnection conn = CreateConnection();
        string query = $"SELECT energy_hourly_log.exact_date_time, energy_usage_log.power_used, energy_usage_log.cost, energy_usage_log.carbon_emission" +
            $" FROM energy_hourly_log INNER JOIN " +
            $"energy_usage_log ON energy_hourly_log.log_id = energy_usage_log.log_id WHERE energy_hourly_log.date = \"{date}\"";
        SQLiteCommand cmd = new SQLiteCommand(query, conn);
        SQLiteDataReader reader = cmd.ExecuteReader();
        List<EnergyUsageLog> hourlyLogCollection = new List<EnergyUsageLog>();
        while (reader.Read())
        {
            try
            {
                string exactDate = reader.GetString(0);
                DateTime time = DateTime.ParseExact(exactDate, "yyyy/MM/dd HH:mm:ss", CultureInfo.InvariantCulture);
                float powerUsed = reader.GetFloat(1);
                float cost = reader.GetFloat(2);
                float carbonEmission = reader.GetFloat(3);
                EnergyUsageLog hourlyLog = new EnergyUsageLog(time, powerUsed, cost, carbonEmission);
                hourlyLogCollection.Add(hourlyLog);
            }
            catch (Exception ex)
            {
                App.GetService<DebugModel>().AddMessage(ex.ToString());
            }
        }
        reader.Close();
        conn.Close();
        return hourlyLogCollection;
    }

    public Dictionary<string, EnergyUsageLog> RetrieveProgramLogByDate(string date)
    {
        SQLiteConnection conn = CreateConnection();
        string query = $"SELECT program_log.program_id, program_log.exact_date_time, energy_usage_log.power_used, energy_usage_log.cost, " +
            $"energy_usage_log.carbon_emission FROM program_log " +
            $"INNER JOIN energy_usage_log ON program_log.log_id = energy_usage_log.log_id " +
            $"WHERE program_log.date = \"{date}\"";
        SQLiteCommand cmd = new SQLiteCommand(query, conn);
        SQLiteDataReader reader = cmd.ExecuteReader();
        Dictionary<string, EnergyUsageLog> programCollection = new Dictionary<string, EnergyUsageLog>();
        while (reader.Read())
        {
            try
            {
                string programID = reader.GetString(0);
                string exactDate = reader.GetString(1);
                DateTime time = DateTime.ParseExact(exactDate, "yyyy/MM/dd HH:mm:ss", CultureInfo.InvariantCulture);
                float powerUsed = reader.GetFloat(2);
                float cost = reader.GetFloat(3);
                float carbonEmission = reader.GetFloat(4);
                EnergyUsageLog programLog = new EnergyUsageLog(time, powerUsed, cost, carbonEmission);
                programCollection.Add(programID, programLog);
            }
            catch (Exception ex)
            {
                App.GetService<DebugModel>().AddMessage(ex.ToString());
            }
        }
        reader.Close();
        conn.Close();
        return programCollection;
    }

    public EnergyUsageDiary RetrieveDiaryByDate(string date)
    {
        try
        {
            EnergyUsageLog dailyLog = RetrieveDailyLogByDate(date);
            List<EnergyUsageLog> hourlyLogs = RetrieveHourlyLogByDate(date);
            Dictionary<string, EnergyUsageLog> programLogs = RetrieveProgramLogByDate(date);
            EnergyUsageDiary diary = new EnergyUsageDiary(dailyLog.Date, dailyLog, hourlyLogs, programLogs);
            return diary;
        }
        catch (Exception ex)
        {
            App.GetService<DebugModel>().AddMessage(ex.ToString());
            return new EnergyUsageDiary();
        }
    }

    public List<EnergyUsageDiary> RetrieveAllDiaries()
    {
        try
        {
            List<EnergyUsageDiary> diaries = new List<EnergyUsageDiary>();
            SQLiteConnection conn = CreateConnection();
            SQLiteCommand cmd = new SQLiteCommand("SELECT date, exact_date_time, daily_log_id FROM energy_diary_log", conn);
            SQLiteDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                string date = reader.GetString(0);
                string exactDate = reader.GetString(1);
                DateTime exactDateTime = DateTime.ParseExact(exactDate, "yyyy/MM/dd HH:mm:ss", CultureInfo.InvariantCulture);
                string daily_log_id = reader.GetString(2);
                EnergyUsageLog dailyLog = ReadFromLogByID(daily_log_id);
                List<EnergyUsageLog> hourlyLogs = RetrieveHourlyLogByDate(date);
                Dictionary<string, EnergyUsageLog> programLogs = RetrieveProgramLogByDate(date);
                diaries.Add(new EnergyUsageDiary(exactDateTime, dailyLog, hourlyLogs, programLogs));
            }
            reader.Close();
            conn.Close();
            return diaries;
        }
        catch (Exception ex)
        {
            App.GetService<DebugModel>().AddMessage(ex.ToString());
            return new List<EnergyUsageDiary>();
        }
    }

    public (double, double) RetrieveLatestBudgetAndCostPerKwh()
    {
        try
        {
            SQLiteConnection connection = CreateConnection();
            string query = "SELECT cost_per_kwh, weekly_budget FROM energy_diary_log ORDER BY diary_log_id DESC LIMIT 1";
            SQLiteCommand cmd = new SQLiteCommand(query, connection);
            SQLiteDataReader reader = cmd.ExecuteReader();
            double costPerKwh = 0;
            double weeklyBudget = 0;
            if (reader.Read())
            {
                costPerKwh = reader.GetDouble(0);
                weeklyBudget = reader.GetDouble(1);
            }
            reader.Close();
            connection.Close();
            return (costPerKwh, weeklyBudget);
        }
        catch (Exception ex)
        {
            App.GetService<DebugModel>().AddMessage(ex.ToString());
            return (0, 0);
        }
    }

    private async Task<EnergyUsageData> ReadUsageDataFromDatabase()
    {
        List<EnergyUsageDiary> diaries = await Task.Run(() => RetrieveAllDiaries());
        double costPerKwh;
        double weeklyBudget;
        (costPerKwh, weeklyBudget) = await Task.Run(() => RetrieveLatestBudgetAndCostPerKwh());
        EnergyUsageData energyUsage = new EnergyUsageData(costPerKwh, weeklyBudget, diaries);
        return energyUsage;
    }

    public async Task<EnergyUsageData> LoadUsageData()
    {
        if (_isInitialized == false)
        {
            await InitializeDB();
        }
        return _energyUsage;
    }

    private async Task<bool> DatabaseIsActive()
    {
        SQLiteConnection conn = CreateConnection();
        string query = "SELECT COUNT(*) FROM energy_diary_log";
        SQLiteCommand cmd = new SQLiteCommand(query, conn);
        int count = Convert.ToInt32(await cmd.ExecuteScalarAsync());
        conn.Close();
        return count > 0;
    }

    private async Task<bool> CheckIfDailyLogExists(SQLiteConnection conn, string date)
    {
        string query = $"SELECT COUNT(*) FROM energy_usage_log WHERE date = \"{date}\" AND type = \"D\"";
        SQLiteCommand cmd = new SQLiteCommand(query, conn);
        int count = Convert.ToInt32(await cmd.ExecuteScalarAsync());
        return count > 0;
    }

    private async Task<bool> CheckIfHourlyLogExists(SQLiteConnection connection, string date, int hour)
    {
        string query = $"SELECT COUNT(*) FROM energy_hourly_log WHERE date = \"{date}\" AND hour = {hour}";
        SQLiteCommand command = new SQLiteCommand(query, connection);
        int count = Convert.ToInt32(await command.ExecuteScalarAsync());
        return count > 0;
    }

    private async Task<bool> CheckIfProgramLogExists(SQLiteConnection connection, string date, string programID)
    {
        string query = $"SELECT COUNT(*) FROM program_log WHERE date = \"{date}\" AND program_id = \"{programID}\"";
        SQLiteCommand command = new SQLiteCommand(query, connection);
        int count = Convert.ToInt32(await command.ExecuteScalarAsync());
        return count > 0;
    }

    private async Task<bool> CheckIfDiaryExists(SQLiteConnection connection, string date)
    {
        string query = $"SELECT COUNT(*) FROM energy_diary_log WHERE date = \"{date}\"";
        SQLiteCommand command = new SQLiteCommand(query, connection);
        int count = Convert.ToInt32(await command.ExecuteScalarAsync());
        return count > 0;
    }

    private async Task DeleteParentLog(SQLiteConnection conn, string table, string date, string additionalInfo)
    {
        string deleteQuery = $"DELETE FROM {table} WHERE date = \"{date}\"  AND {additionalInfo}";
        SQLiteCommand deleteCommand = new SQLiteCommand(deleteQuery, conn);
        await deleteCommand.ExecuteNonQueryAsync();
    }

    private async Task DeleteUsageLog(SQLiteConnection conn, string id)
    {
        string deleteQuery = $"DELETE FROM energy_usage_log WHERE log_id = \"{id}\"";
        SQLiteCommand deleteCommand = new SQLiteCommand(deleteQuery, conn);
        await deleteCommand.ExecuteNonQueryAsync();
    }
}