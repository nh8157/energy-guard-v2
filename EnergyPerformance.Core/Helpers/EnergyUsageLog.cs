using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnergyPerformance.Core.Helpers;

/// <summary>
/// Class to represent invidual log data for energy usage data in an easy-to-use format.
/// </summary>
public class EnergyUsageLog
{

    public DateTime Date
    {
        get; set;
    }
    public float PowerUsed
    {
        get; set;
    }

    // Cost for this specific log, as the user may change their price per kWh value in the future,
    // so we also need to note down the cost value
    public float Cost
    {
        get; set;
    }

    public EnergyUsageLog(DateTime date, float powerUsed, float cost)
    {
        Date = date;
        PowerUsed = powerUsed;
        Cost = cost;
    }
}
