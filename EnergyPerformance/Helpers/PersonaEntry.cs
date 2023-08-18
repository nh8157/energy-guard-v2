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
    public (int, int) CpuSetting
    {
        set; get;
    }
    public int GpuSetting
    {
        set; get;
    }
    
    public float EnergyRating
    {
        set; get;
    }

    public PersonaEntry(int id, string path, float energyRating, (int, int) cpuSetting, int gpuSetting)
    {
        Id = id;
        Path = path;
        CpuSetting = cpuSetting;
        GpuSetting = gpuSetting;
        EnergyRating = energyRating;
    }
}
