namespace EnergyPerformance.Helpers;

public class EnergyRateInfo
{
    private double _energyRate;

    public double EnergyRate
    {
        get => _energyRate;
        set => _energyRate = value;
    }

    public EnergyRateInfo()
    {
        _energyRate = 0;
    }
}
