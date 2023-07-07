using System.Collections.ObjectModel;
using System.ComponentModel;

namespace EnergyPerformance.Models;

public class DebugMessage
{
    public string Message
    {
        get;
        set;
    }

    public DateTime TimeStamp
    {
        get;
        set;
    }

    public DebugMessage(string message)
    {
        Message = message;
        TimeStamp = DateTime.Now;
    }
}

public class DebugModel
{
    public ObservableCollection<DebugMessage> Messages { get; set; } = new();

    public void AddMessage(string message)
    {
        Messages.Add(new DebugMessage(message));
    }
    
    public DebugModel()
    {
        AddMessage("DebugModel created");
    }
}