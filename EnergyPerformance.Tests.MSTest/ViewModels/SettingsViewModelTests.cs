﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using EnergyPerformance.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnergyPerformance.Services;
using Moq;
using EnergyPerformance.Models;
using EnergyPerformance.Contracts.Services;
using EnergyPerformance.Core.Services;
using EnergyPerformance.Core.Contracts.Services;
using Microsoft.Extensions.Options;
using System.Xml.Linq;
using EnergyPerformance.Helpers;

namespace EnergyPerformance.ViewModels.Tests;

[TestClass()]
public class SettingsViewModelTests
{
    private static Mock<IFileService> _fileService;
    private static Mock<IAppNotificationService> _notificationService;
    private static IOptions<LocalSettingsOptions> localSettingsOptions;

    [ClassInitialize]
    public static void ClassInit(TestContext context)
    {
        _fileService = new Mock<IFileService>();
        _notificationService = new Mock<IAppNotificationService>();
        localSettingsOptions = Options.Create(new LocalSettingsOptions());
    }

    public EnergyUsageModel GetModel()
    {
        return new EnergyUsageModel(new EnergyUsageFileService(_fileService.Object));
    }

    public SettingsViewModel GetViewModel()
    {
        var model = GetModel();
        var localSettingsService = new LocalSettingsService(_fileService.Object, localSettingsOptions);
        var cpuInfo = new CpuInfo();
        var cpuTrackerService = new CpuTrackerService(localSettingsService, _notificationService.Object, cpuInfo);
        var themeService = new ThemeSelectorService(localSettingsService);
        var viewModel = new SettingsViewModel(themeService, cpuInfo, localSettingsService, model);
        return viewModel;
    }


    [TestMethod()]
    [DataRow(0.1)]
    [DataRow(0.4)]
    [DataRow(20.0)]
    public void CostSetterTestEqual(double value)
    {
        var viewModel = GetViewModel();
        viewModel.CostPerKwh = value;
        Assert.AreEqual(value, viewModel.CostPerKwh);
    }


    [TestMethod()]
    [DataRow(-1)]
    [DataRow(0)]
    [DataRow(Double.NaN)]
    public void CostSetterTestFails(double value)
    {
        var viewModel = GetViewModel();
        viewModel.CostPerKwh = value;
        Assert.AreNotEqual(value, viewModel.CostPerKwh);
    }



    //[TestMethod()]
    //public void SettingsViewModelTest()
    //{
    //    Assert.Fail();
    //}

    //[TestMethod()]
    //public void LaunchOnStartupActionsTest()
    //{
    //    Assert.Fail();
    //}
}