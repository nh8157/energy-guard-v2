using System.Globalization;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using System.Diagnostics;
namespace EnergyPerformance.Helpers;

/// <summary>
/// Converter class to convert cost calculated from the model to a string in the correct format for display in the View.
/// </summary>
public class CostStringConverter : IValueConverter
{
    public CostStringConverter()
    {
    }

    /// <summary>
    /// Converts float to string with 2 decimal places.
    /// </summary>
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is not null)
        {
            var roundedValue = ((float) value).ToString("0.00");
            return roundedValue;
        }
        return "0.00";
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        // convert back method should not be called for a 1-way binding
        throw new NotImplementedException("ExceptionConvertBackMethodNotImplemented");
    }
}
