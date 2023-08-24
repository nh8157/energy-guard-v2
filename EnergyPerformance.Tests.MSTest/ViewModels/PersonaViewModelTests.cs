using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnergyPerformance.Contracts.Services;
using EnergyPerformance.Core.Contracts.Services;
using EnergyPerformance.Core.Services;
using EnergyPerformance.Helpers;
using EnergyPerformance.Models;
using EnergyPerformance.Services;
using Microsoft.Extensions.Options;
using Moq;
using Windows.Foundation;


namespace EnergyPerformance.ViewModels.Tests;
[TestClass]
public class PersonaViewModelTests
{
    private static Mock<PersonaFileService> _personaFileService;

    [ClassInitialize]
    public static void ClassInit(TestContext context)
    {
        _personaFileService = new Mock<PersonaFileService>();
    }

    public PersonaModel GetModel()
    {
        return new PersonaModel(new CpuInfo(), new PersonaFileService(new FileService()));
    }

    public PersonaViewModel GetViewModel()
    {
        var model = GetModel();
        return new PersonaViewModel(model);
    }

    [TestMethod()]
    public void TestGetModel()
    {
        var model = GetModel();
        Assert.IsNotNull(model);
    }

    [TestMethod()]
    public void TestGetViewModel()
    {
        var viewModel = GetViewModel();
        Assert.IsNotNull(viewModel);
    }

    public async Task TestViewModelInitialize()
    {
        var viewModel = GetViewModel();
    }
}
