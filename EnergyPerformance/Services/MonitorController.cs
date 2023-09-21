using EnergyPerformance.Helpers;

namespace EnergyPerformance.Services;

public class MonitorController
{
    private readonly PipeClient _pipeClient;
    
    public MonitorController(PipeClient pipeClient)
    {
        _pipeClient = pipeClient;
    }
    
    public double GetCpuPower()
    {
        var command = "GetCpuPower";
        var response = _pipeClient.SendAndReceiveMessage(command) ?? "0";
        return double.Parse(response);
    }
    
    public double GetGpuPower()
    {
        var command = "GetGpuPower";
        var response = _pipeClient.SendAndReceiveMessage(command) ?? "0";
        return double.Parse(response);
    }
    
    public double GetGpuUsage()
    {
        var command = "GetGpuUsage";
        var response = _pipeClient.SendAndReceiveMessage(command) ?? "0";
        return double.Parse(response);
    }
}