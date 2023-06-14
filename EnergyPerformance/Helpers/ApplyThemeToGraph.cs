using Microsoft.UI.Xaml;
using OxyPlot;

namespace EnergyPerformance.Helpers;


/// <summary>
/// Extension method for PlotModel to apply a theme.
/// <see href="https://github.com/XamlBrewer/XamlBrewer.WinUI3.OxyPlot.Sample/blob/master/XamlBrewer.WinUI3.OxyPlot.Sample/Services/Theming/PlotModelExtensions.cs">Reference</see>.
/// </summary>
internal static class PlotModelExtensions
{
    /// <summary>
    /// Applies a provided theme to a PlotModel.
    /// </summary>
    /// <param name="plotModel">PlotModel to apply the theme to.</param>
    /// <param name="theme">Theme to apply to the PlotModel.</param>
    public static void ApplyTheme(this PlotModel plotModel, ElementTheme theme)
    {
        // Beware: Do not use OxyColors.Black and OxyColors.White.
        // Their cached brushes are reversed, based on the Theme. Confusing!

        var foreground = theme == ElementTheme.Light ? OxyColor.FromRgb(32, 32, 32) : OxyColors.WhiteSmoke;

        if (plotModel.TextColor != OxyColors.Transparent)
        {
            plotModel.TextColor = foreground;
        }

        foreach (var axis in plotModel.Axes)
        {
            if (axis.TicklineColor != OxyColors.Transparent)
            {
                axis.TicklineColor = foreground;
            }
            if (axis.AxislineColor != OxyColors.Transparent)
            {
                axis.AxislineColor = foreground;
            }
        }

        plotModel.InvalidatePlot(false); // Force the plot to render again.
    }
}
