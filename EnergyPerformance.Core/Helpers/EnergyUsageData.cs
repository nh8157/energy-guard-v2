﻿
namespace EnergyPerformance.Core.Helpers;

/// <summary>
/// Class to represent the energy usage data tracked over time for the application.
/// EnergyUsageModel contains a reference to an instance of this class which is used to store/retrieve all energy usage data.
/// </summary>
public class EnergyUsageData
{
    public double CostPerKwh
    {
        get;
        set;
    }

    public double WeeklyBudget
    {
        get; set;
    }

    public List<EnergyUsageDiary> Diaries
    {
        get; set;
    }


    public EnergyUsageData(double costPerKwh, double weeklyBudget, List<EnergyUsageDiary> diaries)
    {
        CostPerKwh = costPerKwh;
        WeeklyBudget = weeklyBudget;
        Diaries = diaries;
    }

    public EnergyUsageData()
    {
        CostPerKwh = 0;
        WeeklyBudget = 0;
        Diaries = new List<EnergyUsageDiary>();
    }
}