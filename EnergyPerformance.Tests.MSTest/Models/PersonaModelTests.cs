using Moq;
using EnergyPerformance.Models;
using EnergyPerformance.Contracts.Services;
using EnergyPerformance.Core.Services;
using EnergyPerformance.Core.Contracts.Services;
using EnergyPerformance.Core.Helpers;
using Microsoft.Extensions.Options;
using System.Xml.Linq;
using EnergyPerformance.Helpers;
using System.Data.Entity;
using EnergyPerformance.Services;
using System.Diagnostics;

namespace EnergyPerformance.Models.Tests;
[TestClass()]
public class PersonaModelTests
{
    private static List<PersonaEntry> _personaEntries;
    private static PersonaFileService _personaFileService;
    private const string _defaultApplicationDataFolder = "EnergyPerformance/ApplicationData";
    private static FileService _fileService;
    private static string _file;

    private static PersonaEntry generatePersona()
    {
        Random random = new Random();
        var id = random.Next();
        var path = "path";
        var cpuSetting = (random.Next(), random.Next());
        var gpuSetting = random.Next();
        var rating = random.NextDouble();
        var persona = new PersonaEntry(id, path, (float)rating, cpuSetting, gpuSetting);
        return persona;
    }

    [ClassInitialize]
    public static void ClassInit(TestContext context)
    {
        _personaEntries = new List<PersonaEntry>();
        var persona = generatePersona();
        _personaEntries.Add(persona);
        _fileService = new FileService();
        _personaFileService = new PersonaFileService(_fileService);
        var folderPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        folderPath = Path.Combine(folderPath, _defaultApplicationDataFolder);
        var fileName = "Persona.json";
        _file = Path.Combine(folderPath, fileName);
    }

    public PersonaModel GetModel()
    {
        return new PersonaModel(new CpuInfo(), _personaFileService);
    }

    [TestMethod]
    public void TestGetModel()
    {
        var model = GetModel();
        Assert.IsNotNull(model);
    }


    [TestMethod]
    public async Task TestCreate()
    {
        var model = GetModel();
        await model.CreatePersona("path", (float)2);
        var list = model.ReadPersonaAndRating();
        Assert.AreEqual(list.Count(),1);
        var persona = list.FirstOrDefault();
        Assert.AreEqual(persona.Item2, 2);
    }

    [TestMethod]
    public async Task TestUpdate()
    {
        var model = GetModel();
        await model.CreatePersona("path", (float)2);
        var list = model.ReadPersonaAndRating();
        Assert.AreEqual(list.Count(), 1);
        var persona = list.FirstOrDefault();
        Assert.AreEqual(persona.Item2, 2);
        var originRating = persona.Item2;
        await model.UpdatePersona("path", (float)1.5);
        list = model.ReadPersonaAndRating();
        persona = list.FirstOrDefault();
        Assert.AreNotEqual(originRating, persona.Item2);
    }

    [TestMethod]
    public async Task TestDeletion()
    {
        var model = GetModel();
        await model.CreatePersona("path", (float)2);
        await model.DeletePersona("path");
        var list = model.ReadPersonaAndRating();
        Assert.AreEqual(list.Count(), 0);
    }

    [TestMethod]
    public async Task TestEnable()
    {
        var model =  new PersonaModel((new Mock<CpuInfo>()).Object, _personaFileService);
        await model.CreatePersona("path", (float)2);
        model.EnablePersona("path");
        var enabled = model.PersonaEnabled;
        Assert.AreEqual(enabled.EnergyRating, (float)2);
        model.DisableEnabledPersona();
        Assert.AreEqual(model.IsEnabled, false);
    }
}
