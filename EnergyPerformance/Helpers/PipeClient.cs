using System.IO.Pipes;

namespace EnergyPerformance.Helpers;

// Connects to a named pipe server and sends and receives messages from it.
public class PipeClient
{
    private string _pipeName;
    private NamedPipeClientStream _pipeClient;
    
    public PipeClient(string pipeName)
    {
        _pipeName = pipeName;
    }
    
    public void SendMessage(string message)
    {
        using (_pipeClient = new NamedPipeClientStream(".", _pipeName, PipeDirection.InOut))
        {
            _pipeClient.Connect();
            var writer = new StreamWriter(_pipeClient);
            writer.WriteLine(message);
            writer.Flush();
            writer.Close();
        }
    }
    
    public string? ReceiveMessage()
    {
        using (_pipeClient = new NamedPipeClientStream(".", _pipeName, PipeDirection.InOut))
        {
            _pipeClient.Connect();
            var reader = new StreamReader(_pipeClient);
            var response = reader.ReadLine();
            reader.Close();
            return response;
        }
    }
    
    public string SendAndReceiveMessage(string message)
    {
        SendMessage(message);
        return ReceiveMessage();
    }
    
}