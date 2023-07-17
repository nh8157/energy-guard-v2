using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnergyPerformance.Helpers;

namespace EnergyPerformance.Contracts.Services;
public interface ILocationService
{
    public Task<LocationInfo> GetLocationInfo();
}