using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;

namespace EnergyPerformance.Helpers;

/// <summary>
/// Converter class to convert a given theme to an index corresponding to a combobox (dropdown button).
/// </summary>
public class EnumToIndexConverter : IValueConverter
{
    public EnumToIndexConverter()
    {
    }

    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is ElementTheme.Default)
        {
            return 0;
        }
        else if (value is ElementTheme.Light)
        {
            return 1;
        }
        return 2; // ElementTheme.Dark
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        if (parameter is string enumString)
        {
            return Enum.Parse(typeof(ElementTheme), enumString);
        }

        throw new ArgumentException("ExceptionEnumToIndexConverterParameterMustBeAnEnumName");
    }
}
