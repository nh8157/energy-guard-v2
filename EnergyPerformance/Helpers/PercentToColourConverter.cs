using System.Globalization;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using System.Diagnostics;
using Windows.UI;

namespace EnergyPerformance.Helpers;

/// <summary>
/// Converter class to convert a double to a colour based on the value.
/// Used to change the colour of the ProgressRings on the home page.
/// </summary>
public class PercentToColourConverter : IValueConverter
{
    private readonly Color lowColor;
    private const double lowPercent = 33.0;
    private const double mediumPercent = 66.0;
    public PercentToColourConverter()
    {
        // use Fluent UI Green Color instead of default.
        lowColor =  Color.FromArgb(255, 16,137,62);
       
    }

    /// <summary>
    /// Converts a double to a colour based on the value.
    /// </summary>
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is double percent)
        {
            if (percent < lowPercent)
            {
                return new SolidColorBrush(lowColor);
            } else if (percent < mediumPercent)
            {
                return new SolidColorBrush(Colors.Orange);
            } else
            {
                return new SolidColorBrush(Colors.Red);
            }
        }

        throw new ArgumentException("ExceptionPercentToColourConverterParameterMustBeADouble");
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        // convert back method should not be called for a 1-way binding
        throw new NotImplementedException("ExceptionConvertBackMethodNotImplemented");
    }
}
