using System.Globalization;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using System.Diagnostics;
namespace EnergyPerformance.Helpers;

/// <summary>
/// Converter class to convert strings in the EnergyUsage page to localized strings stored in Resources.
/// </summary>
public class EnergyUsageLocalizedStringConverter : IValueConverter
{
    public EnergyUsageLocalizedStringConverter()
    {
    }

    /// <summary>
    /// Converts a given string to its localized version.
    /// </summary>
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        try
        {
            var val = (string) value;
            var key = "EnergyUsageString_"+val;
            return key.GetLocalized();
        }
        catch (Exception e)
        {
            Debug.WriteLine($"Exception: {e.Message}");
            return value;
        }
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        // convert back method should not be called for a 1-way binding
        throw new NotImplementedException("ExceptionConvertBackMethodNotImplemented");
    }
}
