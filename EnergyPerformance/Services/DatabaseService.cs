using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnergyPerformance.Contracts.Services;
using EnergyPerformance.Core.Helpers;
using EnergyPerformance.Models;
using Microsoft.Extensions.Hosting;
using Microsoft.VisualBasic;

namespace EnergyPerformance.Services;
public class DatabaseService: IDatabaseService
{

    private readonly string localApplicationData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
    private readonly string datasource;
    private readonly List<EnergyUsageDiary> _energyUsage;
    public List<EnergyUsageDiary> EnergyUsageDiary => _energyUsage;

    private bool isInitialized;

    public DatabaseService()
    {
        datasource = Path.Combine(localApplicationData, "EnergyPerformance/ApplicationData/database.db");
        _energyUsage = new List<EnergyUsageDiary>();
    }

    public void InitializeDB()
    {
        if (!File.Exists(datasource))
        {
            try
            {
                SQLiteConnection.CreateFile(datasource);
                SQLiteConnection conn = CreateConnection();
                SQLiteCommand sqlite_cmd;
                string CreateEnergyUsageLogTable = "CREATE TABLE energy_usage_log (log_id TEXT NOT NULL PRIMARY KEY , " +
                    "date TEXT NOT NULL, power_used NUMERIC, cost NUMERIC, carbon_emission NUMERIC, type TEXT NOT NULL)";
                string CreateDiaryTable = "CREATE TABLE energy_diary_log (diary_log_id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT , " +
                    "date TEXT NOT NULL, daily_log_id TEXT, FOREIGN KEY (daily_log_id) REFERENCES energy_usage_log(log_id))";
                string CreateProgramLogTable = "CREATE TABLE program_log (date TEXT NOT NULL  , program_id TEXT NOT NULL, " +
                    "log_id TEXT NOT NULL PRIMARY KEY, FOREIGN KEY (log_id) REFERENCES energy_usage_log(log_id))";
                string CreateHourlyLogTable = "CREATE TABLE energy_hourly_log (date TEXT NOT NULL, hour INTEGER NOT NULL, " +
                    "log_id TEXT NOT NULL PRIMARY KEY, FOREIGN KEY (log_id) REFERENCES energy_usage_log(log_id))";
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
    }
    public SQLiteConnection CreateConnection()
    {
        SQLiteConnection sqlite_conn;
        sqlite_conn = new SQLiteConnection($"Data Source={datasource}; Version = 3; New = True; Compress = True; ");
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
    public string InsertNewLog(EnergyUsageLog data, string type)
    {
        try
        {
            SQLiteConnection conn = CreateConnection();
            SQLiteCommand command = conn.CreateCommand();
            command.CommandText = "INSERT INTO energy_usage_log(log_id, date, power_used, cost, carbon_emission, type) VALUES (@log_id, @date, @power_usage, @cost, @carbon_emission, @type)";
            //command.Parameters.AddWithValue("@log_id", data.Date.ToString());
            string id = EncodeID(data.Date, type);
            command.Parameters.AddWithValue("@log_id", id);
            command.Parameters.AddWithValue("@date", data.Date.ToString("yyyy/MM/dd"));
            command.Parameters.AddWithValue("@power_usage", data.PowerUsed);
            command.Parameters.AddWithValue("@cost", data.Cost);
            command.Parameters.AddWithValue("@carbon_emission", data.CarbonEmission);
            command.Parameters.AddWithValue("@type", type);
            command.ExecuteNonQuery();
            conn.Close(); 
            return id;
        }catch (Exception ex)
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
    public async void InsertHourlyLog(int hour, EnergyUsageLog data)
    {
        try
        {
            SQLiteConnection conn = CreateConnection();
            SQLiteCommand cmd = new SQLiteCommand("INSERT INTO energy_hourly_log(date, hour, log_id ) VALUES (@date, @hour, @id)",conn);
            string id = await Task.Run(() => InsertNewLog(data,"H"));
            cmd.Parameters.AddWithValue("@date", data.Date.ToString("yyyy/MM/dd"));
            cmd.Parameters.AddWithValue("@hour", hour);
            cmd.Parameters.AddWithValue("@id", id);
            cmd.ExecuteNonQuery();
            conn.Close();
        } catch (Exception ex)
        {
            App.GetService<DebugModel>().AddMessage(ex.ToString());
        }
    }
    public EnergyUsageLog ReadFromLogByID(string id)
    {
        SQLiteConnection conn = CreateConnection();
        SQLiteCommand cmd = new SQLiteCommand("SELECT * FROM energy_usage_log WHERE log_id = @id ", conn);
        cmd.Parameters.AddWithValue("@id", id);
        SQLiteDataReader reader = cmd.ExecuteReader();
        EnergyUsageLog EnergyLog = new EnergyUsageLog();
        if (reader.Read())
        {
            DateTime date = DateTime.Parse(reader.GetString(1));
            float powerUsed = reader.GetFloat(2);
            float cost = reader.GetFloat(3);
            float carbonEmission = reader.GetFloat(4);
            EnergyLog = new EnergyUsageLog(date, powerUsed, cost, carbonEmission);
        }
        conn.Close();
        return EnergyLog;
    }
    public List<EnergyUsageLog> ReadFromHourlyLogByDay(string date)
    {
        SQLiteConnection conn = CreateConnection();
        string query = $"SELECT energy_hourly_log.hour, energy_usage_log.* FROM energy_hourly_log INNER JOIN energy_usage_log ON energy_hourly_log.log_id = energy_usage_log.log_id WHERE energy_hourly_log.date = \"{date}\"";
        SQLiteCommand cmd = new SQLiteCommand(query, conn);
        SQLiteDataReader reader = cmd.ExecuteReader();
        List<EnergyUsageLog> hourlyLogCollection = new List<EnergyUsageLog>();
        while (reader.Read())
        {
            try
            {
                int hour = reader.GetInt32(0);
                DateTime time = DateTime.ParseExact(date + " " + hour.ToString("00"), "yyyy/MM/dd HH", CultureInfo.InvariantCulture);
                float powerUsed = reader.GetFloat(3);
                float cost = reader.GetFloat(4);
                float carbonEmission = reader.GetFloat(5);
                EnergyUsageLog hourlyLog = new EnergyUsageLog(time, powerUsed, cost, carbonEmission);
                hourlyLogCollection.Add(hourlyLog);
            } catch (Exception ex)
            {
                App.GetService<DebugModel>().AddMessage(ex.ToString());
            }
        }
        conn.Close();
        return hourlyLogCollection;
    }
    public void RetrieveAllDiaries()
    {
        if (!isInitialized)
        {
            SQLiteConnection conn = CreateConnection();
            SQLiteCommand cmd = new SQLiteCommand("SELECT * FROM energy_diary_log", conn);
            SQLiteDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                DateTime date = DateTime.ParseExact(reader.GetString(1), "yyyy/MM/dd", CultureInfo.InvariantCulture);
                string daily_log_id = reader.GetString(2);
                EnergyUsageLog dailyLog = ReadFromLogByID(daily_log_id);
                List<EnergyUsageLog> hourlyLogs = ReadFromHourlyLogByDay(reader.GetString(1));
                Dictionary<string, EnergyUsageLog> programLogs = new Dictionary<string, EnergyUsageLog>();
                _energyUsage.Add(new EnergyUsageDiary(date, dailyLog, hourlyLogs, programLogs));
            }
            isInitialized = true;
            conn.Close();
        }
    }
}
