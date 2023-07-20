using LibreHardwareMonitor.Hardware;

namespace EnergyPerformance.Helpers;

/// <summary>
/// Visitor class used to update the hardware components of the system.
/// </summary>
/// <see href="https://github.com/LibreHardwareMonitor/LibreHardwareMonitor">Reference to LibreHardwareMonitor</see>
public class UpdateVisitor : IVisitor
{
    public void VisitComputer(IComputer computer)
    {
        computer.Traverse(this);
    }
    public void VisitHardware(IHardware hardware)
    {
        hardware.Update();
        foreach (IHardware subHardware in hardware.SubHardware) subHardware.Accept(this);
    }
    public void VisitSensor(ISensor sensor)
    {
    }
    public void VisitParameter(IParameter parameter)
    {
    }
}
