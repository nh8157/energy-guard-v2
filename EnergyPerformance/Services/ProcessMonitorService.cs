using System.Diagnostics;
using System.Management;

namespace EnergyPerformance.Services;

public class ProcessMonitorService
{
    /// <summary>
    /// The following two fields stores the ManagementEventWatchers that monitor and handle
    /// the creation and deletion of processes of interest
    /// </summary>
    private readonly Dictionary<string, ManagementEventWatcher> _creationWatcher;
    private readonly Dictionary<string, ManagementEventWatcher> _deletionWatcher;

    /// <summary>
    /// The fields that store the processes created/deleted
    /// </summary>
    private readonly List<string> _createdProcesses;
    private readonly List<string> _deletedProcesses;

    /// <summary>
    /// This field is a query template that can be used for creation/deletion of event watcher
    /// </summary>
    private const string _query = "TargetInstance isa \"Win32_Process\" AND TargetInstance.Name = '{0}'";

    public event EventHandler? CreationEventHandler;
    public event EventHandler? DeletionEventHandler;

    public string CreatedProcess
    {
        get
        {
            if (_createdProcesses.Count > 0)
            {
                var proc = _createdProcesses[0];
                _createdProcesses.RemoveAt(0);
                return proc;
            }
            return "";
        }
    }

    public string? DeletedProcess
    {
        get
        {
            if (_deletedProcesses.Count > 0)
            {
                var proc = _deletedProcesses[0];
                _deletedProcesses.RemoveAt(0);
                return proc;
            }
            return null;
        }
    }

    public ProcessMonitorService()
    {
        _creationWatcher = new Dictionary<string,ManagementEventWatcher>();
        _deletionWatcher = new Dictionary<string,ManagementEventWatcher>();

        _createdProcesses = new List<string>();
        _deletedProcesses = new List<string>();
    }

    public void AddWatcher(string name)
    {
        if (!_creationWatcher.ContainsKey(name) && !_deletionWatcher.ContainsKey(name))
        {
            var procQuery = string.Format(_query, name);

            var creationQuery = new WqlEventQuery("__InstanceCreationEvent", new TimeSpan(0, 0, 10), procQuery);
            var deletionQuery = new WqlEventQuery("__InstanceDeletionEvent", new TimeSpan(0, 0, 10), procQuery);

            var creationWatcher = new ManagementEventWatcher(creationQuery);
            var deletionWatcher = new ManagementEventWatcher(deletionQuery);

            creationWatcher.EventArrived += GetCreationWatcherHandler(name);
            deletionWatcher.EventArrived += GetDeletionWatcherHandler(name);

            _creationWatcher[name] = creationWatcher;
            _deletionWatcher[name] = deletionWatcher;

            Debug.WriteLine($"Watchers for {name} are created");
        }
    }

    public void RemoveWatcher(string name)
    {
        if (_creationWatcher.ContainsKey(name) && _deletionWatcher.ContainsKey(name))
        {
            _creationWatcher[name].Stop();
            _deletionWatcher[name].Stop();

            _createdProcesses.RemoveAll(proc => proc == name);
            _deletedProcesses.RemoveAll(proc => proc == name);

            Debug.WriteLine($"Watcher for {name} has been stopped");
        }
    }

    public void StartCreationWatcher(string name)
    {
        if (_creationWatcher.ContainsKey(name))
        {
            _creationWatcher[name].Start();
            return;
        }
        throw new ArgumentException($"{name} does not have a creation watcher");
    }

    public void StopCreationWatcher(string name)
    {
        if (_creationWatcher.ContainsKey(name))
        {
            _creationWatcher[name].Stop();
            return;
        }
        throw new ArgumentException($"{name} does not have a creation watcher");
    }

    public void StartDeletionWatcher(string name)
    {
        if (_deletionWatcher.ContainsKey(name))
        {
            _deletionWatcher[name].Start();
            return;
        }
        throw new ArgumentException($"{name} does not have a deletion watcher");
    }

    public void StopDeletionWatcher(string name)
    {
        if (_deletionWatcher.ContainsKey(name))
        {
            _deletionWatcher[name].Stop();
            return;
        }
        throw new ArgumentException($"{name} does not have a deletion watcher");
    }

    private EventArrivedEventHandler GetCreationWatcherHandler(string name)
    {
        return (object sender, EventArrivedEventArgs e) =>
        {
            Debug.WriteLine($"{name} launched");
            _createdProcesses.Add(name);
            _creationWatcher[name].Stop();
            _deletionWatcher[name].Start();

            CreationEventHandler?.Invoke(this, EventArgs.Empty);
        };
    }

    private EventArrivedEventHandler GetDeletionWatcherHandler(string name)
    {
        return (object sender, EventArrivedEventArgs e) =>
        {
            Debug.WriteLine($"{name} exited");
            _deletedProcesses.Add(name);
            _creationWatcher[name].Start();
            _deletionWatcher[name].Stop();

            DeletionEventHandler?.Invoke(this, EventArgs.Empty);
        };
    }
}
