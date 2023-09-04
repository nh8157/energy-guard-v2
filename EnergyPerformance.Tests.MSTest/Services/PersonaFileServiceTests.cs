using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnergyPerformance.Core.Services;
using EnergyPerformance.Services;

namespace EnergyPerformance.Tests.MSTest.Services;
[TestClass]
public class PersonaFileServiceTests
{
    private static FileService _fileService;
    private static PersonaFileService _personaFileService;
    private const string _defaultApplicationDataFolder = "EnergyPerformance/ApplicationData";

    [ClassInitialize]
    public static void ClassInitialize(TestContext context)
    {
        _fileService = new FileService();
        _personaFileService = new PersonaFileService(_fileService);
        var folderPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        folderPath = Path.Combine(folderPath, _defaultApplicationDataFolder);
        var fileName = "PersonaTest.json";


        //_data = new EnergyUsageData(costPerKwh, budget, lastMeasurement, hourlyLogs, dailyLogs);

    }
}
