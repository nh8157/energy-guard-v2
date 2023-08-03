namespace EnergyPerformance.Helpers;
public class PersonaEntry
{
    public int Id
    {
        private set; get;
    }
    public string Path
    {
        set; get;
    }
    public int CpuSetting
    {
        set; get;
    }
    public int GpuSetting
    {
        set; get;
    }

    public PersonaEntry(int id, string path, int cpuSetting, int gpuSetting)
    {
        Id = id;
        Path = path;
        CpuSetting = cpuSetting;
        GpuSetting = gpuSetting;
    }
}
