using System;

using EnergyPerformance.Elevated.MessageHandlers;

namespace EnergyPerformance.Elevated
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Elevated process started");
            // Create a new pipe server
            var pipeServer = new PipeServer("EnergyPerformancePipe");
            Console.WriteLine("Pipe server created");
            // Handle CPU commands
            pipeServer.AddMessageHandler(new CpuHandler());
            // Handle monitor commands
            pipeServer.AddMessageHandler(new MonitorHandler());
            Console.WriteLine("Handlers added");
            // Start the pipe server
            pipeServer.Start();
        }
    }
}
