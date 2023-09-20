using System.Threading.Tasks;

namespace EnergyPerformance.Elevated.MessageHandlers;

public interface MessageHandler
{
    public Task<string?> HandleMessage(string message);
}