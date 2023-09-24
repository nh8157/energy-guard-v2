using System.Threading.Tasks;

namespace EnergyPerformance.Elevated.MessageHandlers;

public interface MessageHandler
{
    public string? HandleMessage(string message);
}