using System.Diagnostics;
using System.Xml.Linq;
using EnergyPerformance.Contracts.Services;
using EnergyPerformance.Core.Contracts.Services;
using EnergyPerformance.Core.Helpers;
using EnergyPerformance.Core.Services;
using EnergyPerformance.Services;

namespace EnergyPerformance.Tests.MSTest;

// Test class to generate random data in Data.json for use when testing the application

[TestClass]
public class DataTestClass
{
    private static EnergyUsageData _data;
    private static IFileService _fileService;
    private const string _defaultApplicationDataFolder = "EnergyPerformance/ApplicationData";

    // Helper method
    private static double RandomDouble(double min, double max)
    {
        // Generate a random number between min and max
        Random random = new Random();
        return (random.NextDouble() * (max - min) + min);
    }

    // Helper method
    private static EnergyUsageLog GenerateRandomDailyLog(DateTime date)
    {
        var log = new EnergyUsageLog(date, (float)RandomDouble(0.5, 2.0), (float)RandomDouble(0.2, 1.5));
        return log;

    }

    private static EnergyUsageLog GenerateRandomHourlyLog(DateTime date)
    {
        var log = new EnergyUsageLog(date, (float)RandomDouble(0.1, 0.5), (float)RandomDouble(0.05, 0.3));
        return log;

    }

    private static List<EnergyUsageLog> GenerateListOfRandomDailyLogs(int days)
    {
        var list = new List<EnergyUsageLog>();
        var startDay = DateTime.Now.AddDays(-1 * days);
        for (var i = 0; i <= days; i++)
        {
            var day = startDay.AddDays(i);
            var log = GenerateRandomDailyLog(day);
            list.Add(log);
        }

        return list;
    }

    private static List<EnergyUsageLog> GenerateListOfRandomHourlyLogs(int days)
    {
        var list = new List<EnergyUsageLog>();
        var startDay = DateTime.Now.AddDays(-1 * days);
        for (var i = 0; i <= days; i++)
        {
            var day = startDay.AddDays(i).AddHours(8);
            for (var j = 0; j < 8; j++)
            {
                var hour = day.AddHours(j);
                var log = GenerateRandomHourlyLog(hour);
                list.Add(log);
            }
        }

        return list;
    }


    private static Dictionary<string, EnergyUsageLog> GenerateRandomPerProcLogs(DateTime date)
    {
        var procDic = new Dictionary<string, EnergyUsageLog>();
        var procList = new List<string>
        {
            "Spotify", "Steam", "Genshin", "Chrome", "Youtube"
        };
        for(var i = 0; i < procList.Count; i++)
        {
            var proc = procList[i];
            var procLog = GenerateRandomHourlyLog(date);
            procDic[proc] = procLog;
        }
        return procDic;
    }

    private static List<EnergyUsageDiary> GenerateListOfRandomEnergyDiaries(int days)
    {
        var list = new List<EnergyUsageDiary>();
        var startDay = DateTime.Now.AddDays(-1 * days + 1);
        for (var i = 0; i < days; i++)
        {
            var day = startDay.AddDays(i).AddHours(8);
            var dailylog = GenerateRandomDailyLog(day);
            var hourlylogs = new List<EnergyUsageLog>();
            var procLogs = GenerateRandomPerProcLogs(day);
            for (var j = 0; j < 8; j++)
            {
                var hour = day.AddHours(j);
                var houlylog = GenerateRandomHourlyLog(hour);
                hourlylogs.Add(houlylog);
            }
            list.Add(new EnergyUsageDiary(day, dailylog, hourlylogs, procLogs));
        }
        return list;
    }


    [ClassInitialize]
    public static async Task ClassInitialize(TestContext context)
    {
        _fileService = new FileService();
        var folderPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        folderPath = Path.Combine(folderPath, _defaultApplicationDataFolder);
        var fileName = "Data.json";
        var costPerKwh = 0.5;
        var budget = RandomDouble(2, 10);
        var diaries = GenerateListOfRandomEnergyDiaries(30);

        _data = new EnergyUsageData(costPerKwh, budget, diaries);
        
        _fileService.Save(folderPath, fileName, _data);
    }

    public async Task<DatabaseService> GetDatabaseService()
    {
        DatabaseService service =  new DatabaseService("testDB.db");
        await service.InitializeDB();
        return service;
    }

    [ClassCleanup]
    public static async Task ClassCleanup()
    {
        Debug.WriteLine("ClassCleanup");
    }

    [TestInitialize]
    public void TestInitialize()
    {
        Debug.WriteLine("TestInitialize");
    }

    [TestCleanup]
    public async Task TestCleanup()
    {
        var service = await GetDatabaseService();
        await service.ClearAllData();
        Debug.WriteLine("TestCleanup");
    }

    [TestMethod]
    public async Task TestMethod()
    {
        // code to run Prime Number stress test through unit test, requires Intel Processor Diagnostic Tool to be installed in Program Files/Intel Corporation folder

        //string programFiles = Environment.ExpandEnvironmentVariables("%ProgramW6432%");
        //string programName = "/Intel Corporation/Intel Processor Diagnostic Tool 64bit/Math_FP.exe";

        //Ref: https://stackoverflow.com/questions/1469764/run-command-prompt-commands
        //Process process = new Process();
        //ProcessStartInfo startInfo = new ProcessStartInfo();
        //startInfo.WindowStyle = ProcessWindowStyle.Hidden;
        //startInfo.FileName = programFiles + programName;
        //startInfo.Arguments = "-s 100";
        //process.StartInfo = startInfo;
        //process.Start();
        // Task.Delay(5000);

        Assert.IsTrue(true);
        await Task.CompletedTask;

        // process.Kill();
    }

    [TestMethod]
    public async Task TestDBInitialization()
    {
        var databaseService = await GetDatabaseService();
        var folderPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        folderPath = Path.Combine(folderPath, _defaultApplicationDataFolder);
        Assert.IsTrue(File.Exists(Path.Combine(folderPath, "testDB.db")));
        var data = await databaseService.LoadUsageData();
        Assert.AreEqual(data.Diaries.Count(), 0);
    }

    [TestMethod]
    public async Task TestReadFromDBSuccess()
    {
        var databaseService = await GetDatabaseService();
        await databaseService.SaveEnergyData(_data);
        var data = await databaseService.LoadUsageData();
        Assert.AreEqual(data.Diaries.Count(), 30);
    }

    [TestMethod]
    public async Task TestReadFromEmptyDB()
    {
        var databaseService = await GetDatabaseService();
        await databaseService.ClearAllData();
        await databaseService.InitializeDB();
        var data = await databaseService.LoadUsageData();
        Assert.AreEqual(data.CostPerKwh,0);
        Assert.AreEqual(data.WeeklyBudget,0);
        Assert.AreEqual(data.Diaries.Count(),0);
    }

    [DataRow(10)]
    [TestMethod]
    public async Task TestUpdateCostAndBudget(double value)
    {
        var databaseService = await GetDatabaseService();
        await databaseService.SaveEnergyData(_data);
        var originalData = await databaseService.LoadUsageData();
        Assert.AreNotEqual(originalData.CostPerKwh,value);
        _data.CostPerKwh = 10;
        await databaseService.SaveEnergyData(_data);
        var newData = await databaseService.LoadUsageData();
        Assert.AreEqual(newData.CostPerKwh, value);
    }

    [TestMethod]
    public async Task TestInsertDailyLog()
    {
        var databaseService = await GetDatabaseService();
        var dailyLog = GenerateRandomDailyLog(DateTime.Now);
        string id = await databaseService.InsertDailyLog(dailyLog);
        Assert.AreNotEqual(id.Length, 0);
    }

    [TestMethod]
    public async Task TestReadFromFile()
    {
        var data = await new EnergyUsageFileService(_fileService).ReadFileAsync();
        Assert.AreEqual(data.CostPerKwh, 0.5);
    }


}
