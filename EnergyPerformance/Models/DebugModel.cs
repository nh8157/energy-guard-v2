using System.Collections.ObjectModel;
using System.ComponentModel;

namespace EnergyPerformance.Models;

public class DebugMessage
{
    // Message colours
    public static readonly string Red = "#CC3333";
    public static readonly string Green = "#33CC33";
    public static readonly string Blue = "#3333CC";
    
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
    
    public string Colour
    {
        get;
        set;
    } = Blue;

    public DebugMessage(string message)
    {
        Message = message;
        TimeStamp = DateTime.Now;
    }
    
    public DebugMessage(string message, string colour)
    {
        Message = message;
        TimeStamp = DateTime.Now;
        Colour = colour;
    }
}

public class DebugModel
{
    public ObservableCollection<DebugMessage> Messages { get; set; } = new();

    public void AddMessage(string message)
    {
        Messages.Add(new DebugMessage(message));
    }
    
    public void AddMessage(string message, string colour)
    {
        Messages.Add(new DebugMessage(message, colour));
    }
    
    public DebugModel()
    {
        AddMessage("DebugModel created");
    }
}