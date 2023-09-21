using System;
using System.Threading.Tasks;
using CLI;

namespace EnergyPerformance.Elevated.MessageHandlers;

public class CpuHandler: MessageHandler
{
    private readonly ManagedController _controller = new();
    
    public string? HandleMessage(string message)
    {
        // The message is expected to be in the format "<command> <arg1> <arg2> ..."
        var args = message.Split(' ');
        var command = args[0];
        var response = "";

        switch (command)
        {
            case "MoveAllAppsToEfficiencyCores":
                _controller.MoveAllAppsToEfficiencyCores();
                break;
            case "MoveAllAppsToSomeEfficiencyCores":
                _controller.MoveAllAppsToSomeEfficiencyCores();
                break;
            case "MoveAppToHybridCores":
                _controller.MoveAppToHybridCores(args[1], int.Parse(args[2]), int.Parse(args[3]));
                break;
            case "MoveAllAppsToHybridCores":
                _controller.MoveAllAppsToHybridCores(int.Parse(args[1]), int.Parse(args[2]));
                break;
            case "ResetToDefaultCores":
                _controller.ResetToDefaultCores();
                break;
            case "DetectCoreCount":
                _controller.DetectCoreCount();
                break;
            case "TotalCoreCount":
                var totalCoreCount = _controller.TotalCoreCount();
                response = totalCoreCount.ToString();
                break;
            case "EfficiencyCoreCount":
                var efficiencyCoreCount = _controller.EfficiencyCoreCount();
                response = efficiencyCoreCount.ToString();
                break;
            case "PerformanceCoreCount":
                var performanceCoreCount = _controller.PerformanceCoreCount();
                response = performanceCoreCount.ToString();
                break;
            default:
                response = null;
                break;
        }
        
        return response;
    }
}