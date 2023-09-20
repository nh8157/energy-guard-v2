using EnergyPerformance.Helpers;
using EnergyPerformance.Services;
using System.Diagnostics;
using System.Management;

namespace EnergyPerformance.Models;

public class PersonaModel
{
    private static float Lerp(float start, float end, float t)
    {
        return start + t * (end - start);
    }

    private static float InverseLerp(float start, float end, float value)
    {
        return (value - start) / (end - start);
    }
    
    private readonly PersonaFileService _personaFileService;
    private readonly ProcessMonitorService _processMonitorService;
    private readonly ProcessTrackerInfo _processTrackerInfo;
    private readonly CpuInfo _cpuInfo;

    /// <summary>
    /// This field stores all existing personas in a list
    /// </summary>
    private List<PersonaEntry> _allPersonas;
    private int _nextPersonaId = 0;

    /// <summary>    
    /// This property indicates whether any persona is enabled right now

    /// </summary>
    public bool IsEnabled
    {
        set; get;
    }

    /// <summary>
    /// This property points to the currently enabled Persona
    /// </summary>
    public PersonaEntry? PersonaEnabled
    {
        set; get;
    }

    public PersonaModel(CpuInfo cpuInfo, PersonaFileService personaFileService, ProcessTrackerInfo processTrackerInfo)
    {
        _personaFileService = personaFileService;
        _allPersonas = new List<PersonaEntry>();
        _processMonitorService = new ProcessMonitorService();
        _processTrackerInfo = processTrackerInfo;
        _cpuInfo = cpuInfo;
        IsEnabled = false;
        PersonaEnabled = null;

        _processMonitorService.CreationEventHandler += CreationEventHandler;
        _processMonitorService.DeletionEventHandler += DeletionEventHandler;
    }

    /// <summary>
    /// This method will be executed in ActivationService, and will read persona data from persistent storage
    /// </summary>
    /// <returns></returns>
    public async Task InitializeAsync()
    {
        _allPersonas = await _personaFileService.ReadFileAsync();

        foreach (var persona in _allPersonas)
        {
            // Add watchers for an existing persona
            _processMonitorService.AddWatcher(persona.Path);

            var processName = GetProcessName(persona.Path);
            if (CheckProcessStatus(processName))
            {
                App.GetService<DebugModel>().AddMessage($"{processName} is running");
                var process = GetProcess(processName);
                // when there is any running instance of the process
                _processTrackerInfo.AddProcess(process);
                _processMonitorService.StartDeletionWatcher(persona.Path);
            }
            else
            {
                // when no instance of the process is running
                App.GetService<DebugModel>().AddMessage($"{processName} is not running");
                _processMonitorService.StartCreationWatcher(persona.Path);
            }

            UpdatePersonaId(persona.Id);
        }
    }

    /// <summary>
    /// The handler is invoked when an application of interest is launched
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public void CreationEventHandler(object? sender, EventArgs e) 
    {
        var executablePath = _processMonitorService.CreatedProcess;

        var processName = GetProcessName(executablePath);

        // Add the process to the tracker service
        if (CheckProcessStatus(processName) && !IsEnabled)
        {
            var process = GetProcess(processName);
            // begin tracking the consumption of the process
            _processTrackerInfo.AddProcess(process);
            
            EnablePersona(executablePath);
            App.MainWindow.DispatcherQueue.TryEnqueue(() =>
            {
                if (executablePath != null)
                {
                    PersonaNotification.EnablePersonaNotification(executablePath);
                }
            });
        }
    }

    /// <summary>
    /// The handler is invoked if an application of interest is closed
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public void DeletionEventHandler(object? sender, EventArgs e)
    {
        var executablePath = _processMonitorService.DeletedProcess;

        if (IsEnabled && PersonaEnabled != null && executablePath != null)
        {
            // first check if the process is still running
            var processName = GetProcessName(executablePath);

            // all instances of the process have stopped
            if (!CheckProcessStatus(processName) && executablePath == PersonaEnabled.Path)
            {
                _processTrackerInfo.RemoveProcess(processName);
                DisableEnabledPersona();
                App.MainWindow.DispatcherQueue.TryEnqueue(() =>
                {
                    if (executablePath != null)
                    {
                        PersonaNotification.DisabledPersonaNotification(executablePath);
                    }
                });
            }
        }
        else
        {
            Debug.WriteLine($"No persona enabled");
        }
    }

    /// <summary>
    /// This methods creates a new Persona file based on the path and energy rating the user sets
    /// It converts the energy rating into respective CPU and GPU settings based on the hardware spec
    /// It assigns every Persona a unique ID (this can be swapped with the unique ID returned by the database)
    /// </summary>
    /// <param name="personaName">The name of the Persona</param>
    /// <param name="energyRating">The position of the slider defined by the user</param>
    public async Task CreatePersona(string personaName, float energyRating)
    {
        if (!_allPersonas.Any(persona => persona.Path.Equals(personaName, StringComparison.OrdinalIgnoreCase)))
        {
            var entry = new PersonaEntry(_nextPersonaId, personaName, energyRating,
                ConvertRatingToCpuSetting(energyRating), ConvertRatingToGpuSetting(energyRating));

            _allPersonas.Add(entry);

            // Add creation and deletion watchers for the application
            _processMonitorService.AddWatcher(personaName);

            await _personaFileService.SaveFileAsync();
            _nextPersonaId += 1;
        }
    }

    /// <summary>
    /// This method reads all existing Personas with their respective energy ratings
    /// </summary>
    /// <returns>List of executable path name and energy rating pair for every application</returns>
    public List<(string, float)> ReadPersonaAndRating()
    {
        List<(string, float)> profiles = _allPersonas.Select(persona => 
            (persona.Path, persona.EnergyRating)
        ).ToList();
        return profiles;
    }

    /// <summary>
    /// This method is invoked when the user updates the setting on the frontend
    /// </summary>
    /// <param name="personaName">The name of the Persona</param>
    /// <param name="energyRating">User-defined slider position</param>
    public async Task UpdatePersona(string personaName, float energyRating)
    {
        _allPersonas.ForEach(persona =>
        {
            if (persona.Path.Equals(personaName, StringComparison.OrdinalIgnoreCase))
            {
                Debug.WriteLine($"Updating persona {personaName} to {energyRating}");
                persona.CpuSetting = ConvertRatingToCpuSetting(energyRating);
                persona.GpuSetting = ConvertRatingToGpuSetting(energyRating);
                persona.EnergyRating = energyRating;
                // If persona is enabled, reapply the affinity mask
                if (PersonaEnabled != null && IsEnabled && 
                    PersonaEnabled.Path.Equals(personaName, StringComparison.OrdinalIgnoreCase))
                {
                    _cpuInfo.EnableCpuSetting(persona.Path, persona.CpuSetting);
                }
                
                _processMonitorService.RemoveWatcher(personaName);
                _processMonitorService.AddWatcher(personaName);
            }
        });
        await _personaFileService.SaveFileAsync();
    }

    /// <summary>
    /// Removes the Persona from the volatile and persistent memory
    /// </summary>
    /// <param name="personaName">The name of the Persona</param>
    public async Task DeletePersona(string personaName)
    {
        _processMonitorService.RemoveWatcher(personaName);
        var res = _allPersonas.RemoveAll(persona => persona.Path.Equals(personaName, StringComparison.OrdinalIgnoreCase)) > 0;
        await _personaFileService.SaveFileAsync();
    }

    /// <summary>
    /// Applies the CPU/GPU settings that correspond to the process to the hardware
    /// </summary>
    /// <param name="personaName">The name of the Persona</param>
    /// <returns>Whether the enabling was successful</returns>
    public bool EnablePersona(string personaName)
    {
        var index = _allPersonas.FindIndex(persona => persona.Path.Equals(personaName, StringComparison.OrdinalIgnoreCase));

        if (index != -1)
        {
            var persona = _allPersonas[index];
            // we need to research how to verify if a process is currently running
            // if it is not running, then the following lines should not be executed
            PersonaEnabled = persona;
            IsEnabled = true;

            // pass the settings to CPU and GPU
            _cpuInfo.EnableCpuSetting(persona.Path, persona.CpuSetting);

            return true;
        }
        return false;
    }

    /// <summary>
    /// Invoked when the user exits the selected program
    /// Disable the engaged Persona setting
    /// </summary>
    /// <param name="personaName"></param>
    /// <returns></returns>
    public bool DisableEnabledPersona()
    {
        if (PersonaEnabled != null)
        {
            var personaName = PersonaEnabled.Path;

            if (PersonaEnabled == null)
            {
                return false;
            }
            _cpuInfo.DisableCpuSetting(personaName, PersonaEnabled.CpuSetting);

            PersonaEnabled = null;
            IsEnabled = false;

            return true;
        }
        return false;
    }

    // The conversion works as follows: the value of the slider is between 1 and 3 where
    // 1 is the highest energy saving setting and 3 is the highest performance setting.
    // For the lowest energy setting we want to use a single performance core, for 2
    // we want to use all performance cores and for  3  we want to
    // use all performance cores and all efficiency cores.
    // The idea is that the slider is linearly mapped to the number of cores.
    private (int, int) ConvertRatingToCpuSetting(float energyRating)
    {
        var numEfficiencyCores = _cpuInfo.CpuController.EfficiencyCoreCount();
        var numPerformanceCores = _cpuInfo.CpuController.PerformanceCoreCount();
        
        if(energyRating < 1 || energyRating > 3)
        {
            throw new ArgumentException("Energy rating must be between 1 and 3");
        }

        // Interpolate between one core and the total number of efficiency cores
        if (energyRating >= 2)
        {
            var setEfficiencyCores = (int)Math.Round(Lerp(1, numEfficiencyCores, 3-energyRating));
            return (setEfficiencyCores, 0);
        }

        var setPerformanceCores = (int)Math.Round(Lerp(0, numPerformanceCores, 2-energyRating));
        return (numEfficiencyCores, setPerformanceCores);
    }

    
    private int ConvertRatingToGpuSetting(float energyRating)
    {
        return 0;
    }

    /// <summary>
    /// This method removes the .exe suffix from the path
    /// </summary>
    /// <param name="path">Original path name</param>
    /// <returns>Trimmed path name</returns>
    private string GetProcessName(string path)
    {
        var processName = Path.GetFileName(path);
        if (processName.Contains(".exe"))
            processName = processName.Remove(processName.Length - ".exe".Length);
        return processName;
    }

    /// <summary>
    /// Retrieves the first instance of the Process object
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    private Process GetProcess(string path)
    {
        return Process.GetProcessesByName(path).First();
    }

    /// <summary>
    /// Checks if there is any running instance of a process
    /// </summary>
    /// <param name="path">The name of the process</param>
    /// <returns>True if the process is running, false otherwise</returns>
    private bool CheckProcessStatus(string path)
    {
        var proc = Process.GetProcessesByName(path).FirstOrDefault();
        return (proc == null) ? false : true;
    }

    /// <summary>
    /// Computes the next available ID for a persona
    /// </summary>
    /// <param name="id">The current ID</param>
    private void UpdatePersonaId(int id)
    {
        if (id >= _nextPersonaId)
        {
            _nextPersonaId = id + 1;
        }
    }
}
