using EnergyPerformance.Helpers;

namespace EnergyPerformance.Services;

public class MonitorController
{
    private readonly PipeClient _pipeClient;
    
    public MonitorController(PipeClient pipeClient)
    {
        _pipeClient = pipeClient;
    }
    
    public double getCpuPower()
    {
        var command = "getCpuPower";
        var response = _pipeClient.SendAndReceiveMessage(command) ?? "0";
        return double.Parse(response);
    }
    
    public double getGpuPower()
    {
        var command = "getGpuPower";
        var response = _pipeClient.SendAndReceiveMessage(command) ?? "0";
        return double.Parse(response);
    }
    
    public double getGpuUsage()
    {
        var command = "getGpuUsage";
        var response = _pipeClient.SendAndReceiveMessage(command) ?? "0";
        return double.Parse(response);
    }
}