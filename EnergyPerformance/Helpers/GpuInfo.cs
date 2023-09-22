﻿using System.ComponentModel;

namespace EnergyPerformance.Helpers;

public class GpuInfo
{
    private double _gpuUsage;
    private double _gpuPower;

    public event PropertyChangedEventHandler? GpuUsageChanged;

    public double GpuUsage
    {
        get => _gpuUsage;
        set
        {
            _gpuUsage = value;
            GpuUsageChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(GpuUsage)));
        }
    }

    public double GpuPower
    {
        get => _gpuPower;
        set => _gpuPower = value;
    }

    /// <summary>
    /// The current CPU usage of each process running on the system.
    /// </summary>
    public virtual Dictionary<string, double> ProcessesGpuUsage { get; set; }
    
    public GpuInfo()
    {
        ProcessesGpuUsage = new Dictionary<string, double>();
        GpuUsage = 0;
        GpuPower = 0;
    }
}