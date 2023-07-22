using System.Management;
using System.Reflection.Metadata.Ecma335;
using EnergyPerformance.Models;

namespace EnergyPerformance.Services;

public class ProcessMonitorService
{
    /// <summary>
    /// The following two fields stores the ManagementEventWatchers that monitor and handle
    /// the creation and deletion of processes of interest
    /// </summary>
    private Dictionary<string, ManagementEventWatcher> _creationWatcher;
    private Dictionary<string, ManagementEventWatcher> _deletionWatcher;

    /// <summary>
    /// This field is a query template that can be used for creation/deletion of event watcher
    /// </summary>
    private const string _query = "TargetInstance isa \"Win32_Process\" AND TargetInstance.Name = '{0}'";

    public ProcessMonitorService()
    {
        _creationWatcher = new Dictionary<string,ManagementEventWatcher>();
        _deletionWatcher = new Dictionary<string,ManagementEventWatcher>();
    }

    public bool AddWatcher(string name)
    {
        if (!_creationWatcher.ContainsKey(name) && !_deletionWatcher.ContainsKey(name))
        {
            var procQuery = String.Format(_query, name);
            var creationQuery = new WqlEventQuery("__InstanceCreationEvent", new TimeSpan(0, 0, 1), procQuery);
            var deletionQuery = new WqlEventQuery("__InstanceDeletionEvent", new TimeSpan(0, 0, 1), procQuery);

            var creationWatcher = new ManagementEventWatcher(creationQuery);
            var deletionWatcher = new ManagementEventWatcher(deletionQuery);

            creationWatcher.EventArrived += GetCreationWatcherHandler(name);
            deletionWatcher.EventArrived += GetDeletionWatcherHandler(name);

            _creationWatcher[name] = creationWatcher;
            _deletionWatcher[name] = deletionWatcher;

            creationWatcher.Start();
            return true;
        }
        return false;
    }

    public bool RemoveWatcher(string name)
    {
        return false;
    }

    private EventArrivedEventHandler GetCreationWatcherHandler(string name)
    {
        return (object sender, EventArrivedEventArgs e) =>
        {
            _creationWatcher[name].Stop();
            _deletionWatcher[name].Start();
        };
    }

    private EventArrivedEventHandler GetDeletionWatcherHandler(string name)
    {
        return (object sender, EventArrivedEventArgs e) =>
        {
            _creationWatcher[name].Start();
            _deletionWatcher[name].Stop();
        };
    }
}
