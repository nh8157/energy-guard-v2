using System.Diagnostics;
using EnergyPerformance.Helpers;

namespace EnergyPerformance.Services;

public sealed class Controller
{
    private readonly PipeClient _pipeClient;
    
    public Controller(PipeClient pipeClient)
    {
        _pipeClient = pipeClient;
    }
    
    public void MoveAllAppsToEfficiencyCores()
    {
        var command = "MoveAllAppsToEfficiencyCores";
        _pipeClient.SendMessage(command);
    }

    public void MoveAllAppsToSomeEfficiencyCores()
    {
        var command = "MoveAllAppsToSomeEfficiencyCores";
        _pipeClient.SendMessage(command);
    }
    
    public bool MoveAppToHybridCores(string target, int eCores, int pCores) 
    {
        var command = $"MoveAppToHybridCores {target} {eCores} {pCores}";
        var response = _pipeClient.SendAndReceiveMessage(command);
        return response == "true";
    }

    public void MoveAllAppsToHybridCores(int eCores, int pCores)
    {
        var command = $"MoveAllAppsToHybridCores {eCores} {pCores}";
        _pipeClient.SendMessage(command);
    }

    public void ResetToDefaultCores()
    {
        var command = "ResetToDefaultCores";
        _pipeClient.SendMessage(command);
    }

    public void DetectCoreCount()
    {
        var command = "DetectCoreCount";
        _pipeClient.SendMessage(command);
    }

    public int TotalCoreCount()
    {
        var command = "TotalCoreCount";
        var response = _pipeClient.SendAndReceiveMessage(command);
        return int.Parse(response);
    }

    public int EfficiencyCoreCount()
    {
        var command = "EfficiencyCoreCount";
        var response = _pipeClient.SendAndReceiveMessage(command);
        return int.Parse(response);
    }

    public int PerformanceCoreCount()
    {
        var command = "PerformanceCoreCount";
        var response = _pipeClient.SendAndReceiveMessage(command);
        Debug.WriteLine(response);
        return int.Parse(response);
    }
}
