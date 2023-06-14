using System.Globalization;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using System.Diagnostics;
namespace EnergyPerformance.Helpers;

/// <summary>
/// Converter class for converting budget calculated in model to string for display in the View.
/// </summary>
public class BudgetStringConverter : IValueConverter
{
    public BudgetStringConverter()
    {
    }

    /// <summary>
    /// Converts a double to a string with no decimal places.
    /// </summary>
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is not null)
        {
            var roundedValue = ((double) value).ToString("0");
            return roundedValue;
        }
        // throw new ArgumentException("ExceptionCpuUsageToColourConverterParameterMustBeADouble");
        return "0";

        
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        // convert back method should not be called for a 1-way binding
        throw new NotImplementedException("ExceptionConvertBackMethodNotImplemented");
    }
}
