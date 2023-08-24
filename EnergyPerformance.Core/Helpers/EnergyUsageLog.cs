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

    public float CarbonEmission
    {
        get; set;
    }

    public override bool Equals(object obj)
    {
        if (obj == null)
        {
            return false;
        }
        if (!(obj is EnergyUsageLog))
        {
            return false;
        }
        return this.Date.Year == ((EnergyUsageLog)obj).Date.Year &&
            this.Date.Month == ((EnergyUsageLog)obj).Date.Month &&
            this.Date.Minute == ((EnergyUsageLog)obj).Date.Minute &&
            this.Date.Second == ((EnergyUsageLog)obj).Date.Second &&
            this.PowerUsed == ((EnergyUsageLog)obj).PowerUsed &&
            this.Cost == ((EnergyUsageLog)obj).Cost &&
            this.CarbonEmission == ((EnergyUsageLog)obj).CarbonEmission;
    }

    public EnergyUsageLog(DateTime date, float powerUsed, float cost, float carbonEmission = 0)
    {
        Date = date;
        PowerUsed = powerUsed;
        Cost = cost;
        CarbonEmission = carbonEmission;
    }

    public EnergyUsageLog()
    {
        Date = DateTime.Now;
        PowerUsed = 0;
        Cost = 0;
        CarbonEmission = 0;
    }
}