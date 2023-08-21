using System.IO.Pipes;

namespace EnergyPerformance.Helpers;

// Connects to a named pipe server and sends and receives messages from it.
public class PipeClient
{
    private string _pipeName;
    
    public PipeClient(string pipeName)
    {
        _pipeName = pipeName;
    }
    
    public void SendMessage(string message)
    {
        using var pipeClient = new NamedPipeClientStream(".", _pipeName, PipeDirection.Out);
        pipeClient.Connect();
        var writer = new StreamWriter(pipeClient);
        writer.Write(message);
        writer.Flush();
        writer.Close();
    }
    
    public string? ReceiveMessage()
    {
        using var pipeClient = new NamedPipeClientStream(".", _pipeName, PipeDirection.In);
        pipeClient.Connect();
        var reader = new StreamReader(pipeClient);
        var response = reader.ReadLine();
        reader.Close();
        return response;
    }
    
    public string? SendAndReceiveMessage(string message)
    {
        using var pipeClient = new NamedPipeClientStream(".", _pipeName, PipeDirection.InOut);
        pipeClient.Connect();
        var writer = new StreamWriter(pipeClient);
        writer.Write(message);
        writer.Flush();
        var reader = new StreamReader(pipeClient);
        var response = reader.ReadLine();
        writer.Close();
        reader.Close();
        return response;
    }
    
}