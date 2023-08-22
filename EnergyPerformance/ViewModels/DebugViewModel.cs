using System.Collections.ObjectModel;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using EnergyPerformance.Models;

namespace EnergyPerformance.ViewModels;

public class DebugViewModel : ObservableRecipient
{
    public readonly DebugModel Model;
    public readonly ObservableCollection<DebugMessage> Messages;

    public DebugViewModel(DebugModel model)
    {
        Model = model;
        Messages = model.Messages;
    }
}