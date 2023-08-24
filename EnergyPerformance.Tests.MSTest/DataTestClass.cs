using System.Data.SQLite;
using System.Diagnostics;
using System.Xml.Linq;
using EnergyPerformance.Contracts.Services;
using EnergyPerformance.Core.Contracts.Services;
using EnergyPerformance.Core.Helpers;
using EnergyPerformance.Core.Services;
using EnergyPerformance.Helpers;
using EnergyPerformance.Services;
using EnergyPerformance.Wrappers;
using Moq;
using Windows.Media.PlayTo;

namespace EnergyPerformance.Tests.MSTest;

// Test class to generate random data in Data.json for use when testing the application

[TestClass]
public class DataTestClass
{
    private static EnergyUsageData _data;
    private static IFileService _fileService;
    private const string _defaultApplicationDataFolder = "EnergyPerformance/ApplicationData";
    private static string _filepath;

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
            procDic.Add(proc, procLog);
        }
        return procDic;
    }

    public static List<EnergyUsageDiary> GenerateListOfRandomEnergyDiaries(int days)
    {
        var list = new List<EnergyUsageDiary>();
        var startDay = DateTime.Now.AddDays(-1 * days + 1);
        startDay = new DateTime(startDay.Year, startDay.Month, startDay.Day, 0, startDay.Minute, startDay.Second);
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
        var costPerKwh = 0.5;
        var budget = RandomDouble(2, 10);
        var diaries = GenerateListOfRandomEnergyDiaries(30);
        var folderPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var directory = Path.Combine(folderPath, _defaultApplicationDataFolder);
        _filepath= Path.Combine(directory, "testDB.db");
        if (File.Exists(_filepath))
        {
            SQLiteConnection connection = new SQLiteConnection("Data Source=" + _filepath + ";Version=3;");
            connection.Close();
            GC.Collect();
            GC.WaitForPendingFinalizers();
            File.Delete(_filepath);
        }
        _data = new EnergyUsageData(costPerKwh, budget, diaries);
    }

    public DatabaseService GetDatabaseService()
    {
        DatabaseService service =  new DatabaseService("testDB.db");
        return service;
    }

    [ClassCleanup]
    public static void ClassCleanup()
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
        var service = GetDatabaseService();
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
    public async Task TestDBInitializationWhenNoDirectory()
    {
        var folderPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var directory = Path.Combine(folderPath, _defaultApplicationDataFolder);
        var wrapper = new Mock<DatabaseMethodFactory>();
        wrapper.Setup(w => w.DirectoryExists(directory)).Returns(false);
        var databaseService = GetDatabaseService();
        databaseService.MethodWrapper = wrapper.Object;
        await databaseService.InitializeDB();
        Assert.IsTrue(File.Exists(Path.Combine(directory, "testDB.db")));
    }

    [TestMethod]
    public async Task TestDBInitializationWhenNoDB()
    {
        var databaseService = GetDatabaseService();
        var folderPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var directory = Path.Combine(folderPath, _defaultApplicationDataFolder);
        var filepath = Path.Combine(directory, "testDB.db");
        var wrapper = new Mock<DatabaseMethodFactory>();
        wrapper.Setup(w => w.FileExists(filepath)).Returns(false);
        await databaseService.InitializeDB();
        Assert.IsTrue(File.Exists(filepath));
        var data = await databaseService.LoadUsageData();
        Assert.AreEqual(data.Diaries.Count(), 0);
    }

    [TestMethod]
    public async Task TestInitializationWhenDBExists()
    {
        var databaseService = GetDatabaseService();
        await databaseService.SaveEnergyData(_data);
        databaseService = GetDatabaseService();
        var wrapper = new Mock<DatabaseMethodFactory>();
        wrapper.Setup(w => w.DatabaseIsActive(databaseService.CreateConnection())).ReturnsAsync(true);
        await databaseService.InitializeDB();
        var data = await databaseService.LoadUsageData();
        Assert.AreEqual(data.Diaries.Count(),30);
    }

    [TestMethod]
    public async Task TestReadFromDBSuccess()
    {
        var databaseService = GetDatabaseService();
        await databaseService.SaveEnergyData(_data);
        var data = await databaseService.LoadUsageData();
        Assert.AreEqual(data.Diaries.Count(), 30);
    }

    [TestMethod]
    public async Task TestReadFromEmptyDB()
    {
        var databaseService = GetDatabaseService();
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
        var databaseService =  GetDatabaseService();
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
        var databaseService = GetDatabaseService();
        var dailyLog = GenerateRandomDailyLog(DateTime.Now);
        string id = await databaseService.InsertDailyLog(dailyLog);
        Assert.AreNotEqual(id.Length, 0);
    }

    [DataRow(0)]
    [DataRow(-2)]
    [DataRow(-15)]
    [TestMethod]
    public async Task TestRetrieveDiaryByDate(int value)
    {
        var databaseService = GetDatabaseService();
        await databaseService.SaveEnergyData(_data);
        var timeOfData = _data.Diaries[^1].Date;
        var targetDate = timeOfData.AddDays(value);
        var targetDiary = databaseService.RetrieveDiaryByDate(targetDate.ToString("yyyy/MM/dd"));
        var index = -value + 1;
        Assert.IsTrue(object.Equals(targetDiary, _data.Diaries[^index]));
    }

}
