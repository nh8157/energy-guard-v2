using Microsoft.VisualStudio.TestTools.UnitTesting;
using EnergyPerformance.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnergyPerformance.Services;
using Moq;
using EnergyPerformance.Models;
using EnergyPerformance.Contracts.Services;
using EnergyPerformance.Core.Services;
using EnergyPerformance.Core.Contracts.Services;
using EnergyPerformance.Core.Helpers;
using Microsoft.Extensions.Options;
using System.Xml.Linq;
using EnergyPerformance.Helpers;
using EnergyPerformance.Notifications;
using System.Linq.Expressions;

namespace EnergyPerformance.ViewModels.Tests;

[TestClass()]
public class EnergyUsageViewModelTests
{
    private static Mock<IFileService> _fileService;
    private static IOptions<LocalSettingsOptions> localSettingsOptions;
    private static CarbonIntensityInfo _carbonIntensityInfo;
    private static Mock<IDatabaseService> _databaseService;
    private static EnergyRateInfo _energyRateInfo;

    [ClassInitialize]
    public static void ClassInit(TestContext context)
    {
        _carbonIntensityInfo = new CarbonIntensityInfo();
        _databaseService = new Mock<IDatabaseService>();
        _energyRateInfo = new EnergyRateInfo();
        _fileService = new Mock<IFileService>();
        localSettingsOptions = Options.Create(new LocalSettingsOptions());
    }

    public EnergyUsageModel GetModel()
    {
        return new EnergyUsageModel(_carbonIntensityInfo, _energyRateInfo, _databaseService.Object);
    }

    public EnergyUsageViewModel GetViewModel()
    {
        var model = GetModel();
        var viewModel = new EnergyUsageViewModel(model);
        return viewModel;
    }

    [TestMethod()]
    public void TestViewModelIsInitializedCorrectly()
    {
        var viewModel = GetViewModel();
        Assert.IsNotNull(viewModel.Model);
        Assert.AreEqual(viewModel.Model.PlotAreaBorderThickness.Right, 0.0);
        Assert.AreEqual(viewModel.Model.DefaultFont, "Segoe UI");
        Assert.AreEqual(viewModel.Model.Axes.First().Title, "Cost");
    }

    // write a unit test for the cost model function in the energy usage view model
    [TestMethod()]
    public void TestCostModelAndLineLimitFunction()
    {
        var viewModel = GetViewModel();
        viewModel.CostModel();
        Assert.AreEqual(viewModel.Model.Annotations.Count, 1);
    }

    [TestMethod()]
    public void TestSwitchEnergyModelDaily()
    {
        var viewModel = GetViewModel();
        viewModel.CurrentMode = "Energy";
        viewModel.CurrentTimeSpan = "Daily";
        viewModel.SwitchCostEnergyModel();
        Assert.AreEqual(viewModel.Model.Axes.First().Unit, "£");
        Assert.AreEqual(viewModel.Model.Annotations.Count, 1);
    }


    [TestMethod()]
    public void TestSwitchEnergyModelAndTimeSpanHourly()
    {
        var viewModel = GetViewModel();
        viewModel.CurrentMode = "Energy";
        viewModel.CurrentTimeSpan = "Hourly";
        viewModel.SwitchTimeSpan();
        Assert.AreEqual(viewModel.Model.Axes.First().Unit, "kWh");
        Assert.AreEqual(viewModel.Model.Annotations.Count, 1);
    }

    [TestMethod()]
    public void TestSwitchCostModelDaily()
    {
        var viewModel = GetViewModel();
        viewModel.CurrentMode = "Cost";
        viewModel.CurrentTimeSpan = "Daily";
        viewModel.SwitchTimeSpan();
        Assert.AreEqual(viewModel.Model.Axes.First().Unit, "£");
        Assert.AreEqual(viewModel.Model.Annotations.Count, 0);
    }

    [TestMethod()]
    public void TestSwitchCostModelHourly()
    {
        var viewModel = GetViewModel();
        viewModel.CurrentMode = "Cost";
        viewModel.CurrentTimeSpan = "Hourly";
        viewModel.SwitchTimeSpan();
        Assert.AreEqual(viewModel.Model.Axes.First().Unit, "£");
        Assert.AreEqual(viewModel.Model.Annotations.Count, 1);
    }

    [TestMethod]
    public void TestSwitchCostModelWithHourly()
    {
        var viewModel = GetViewModel();
        viewModel.CurrentMode = "Cost";
        viewModel.CurrentTimeSpan = "Daily";
        viewModel.SwitchTimeSpan();
        viewModel.SwitchCostEnergyModel();
        Assert.AreEqual(viewModel.Model.Axes.First().Unit, "kWh");
        Assert.AreEqual(viewModel.Model.Annotations.Count, 0);
    }

    [TestMethod]
    public void TestSwitchEnergyModelWithHourly()
    {
        var viewModel = GetViewModel();
        viewModel.CurrentMode = "Energy";
        viewModel.CurrentTimeSpan = "Daily";
        viewModel.SwitchTimeSpan();
        viewModel.SwitchCostEnergyModel();
        Assert.AreEqual(viewModel.Model.Axes.First().Unit, "£");
        Assert.AreEqual(viewModel.Model.Annotations.Count, 0);
    }


    [TestMethod]
    public void TestSwitchCostModelWithDaily()
    {
        var viewModel = GetViewModel();
        viewModel.CurrentMode = "Cost";
        viewModel.CurrentTimeSpan = "Daily";
        viewModel.SwitchCostEnergyModel();
        Assert.AreEqual(viewModel.Model.Axes.First().Unit, "kWh");
        Assert.AreEqual(viewModel.Model.Annotations.Count, 1);
    }



    [TestMethod()]
    public void TestSwitchEnergyModelHourly()
    {
        var viewModel = GetViewModel();
        viewModel.CurrentMode = "Energy";
        viewModel.CurrentTimeSpan = "Daily";
        viewModel.SwitchTimeSpan();
        Assert.AreEqual(viewModel.Model.Axes.First().Unit, "kWh");
        Assert.AreEqual(viewModel.Model.Annotations.Count, 0);
    }


}