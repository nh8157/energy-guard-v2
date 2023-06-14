using System.Collections.Specialized;

namespace EnergyPerformance.Contracts.Services;

public interface IAppNotificationService
{
    void Initialize();

    bool Show(string payload);

    NameValueCollection ParseArguments(string arguments);

    void Unregister();
    Task ShowAsync(string v);
}
