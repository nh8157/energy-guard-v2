using EnergyPerformance.Helpers;
using EnergyPerformance.Models;

namespace EnergyPerformance.Services;
public class AutoConfigurationService
{
    private int _left;
    private int _right;
    private string _executable = ""; // Application being auto-configured
    private readonly List<float> _personaEnergyRates; // Enery rates in the persona customization slider
    private int smoothnessCheckCount = 0; // The number of times the smoothness check has been inquired to the user.

    private const int delayInMinutes = 3; // Minutes till the next smoothness check

    public AutoConfigurationService()
    {
        // Create a list containing the Persona energy rates
        _personaEnergyRates = new List<float>();
        for (var i = 1.0f; i <= 3.0; i += 0.1f)
        {
            i = (float)Math.Round(i, 1);
            _personaEnergyRates.Add(i);
        }
    }

    public async void Start(string executable)
    {
        _executable = executable;

        _left = 0;
        _right = _personaEnergyRates.Count;

        var mid = _left + ((_right - _left) / 2);

        await App.GetService<PersonaModel>().UpdatePersona(_executable, _personaEnergyRates[mid]);
        await Task.Delay(TimeSpan.FromMinutes(delayInMinutes));

        PersonaNotification.SmoothnessCheckNotification(_executable, ++smoothnessCheckCount);
    }


    /// <summary>
    /// Auto-configure the target application's Persona rate using Binary Search.
    /// </summary>
    /// <param name="isApplicationRunningSmoothly">
    /// True if application is running smoothly, False otherwise.
    /// </param>
    public async void AutoConfigure(bool isApplicationRunningSmoothly)
    {
        if (_left < _right)
        {
            // Get the mid point
            var mid = _left + ((_right - _left) / 2);

            await App.GetService<PersonaModel>().UpdatePersona(_executable, _personaEnergyRates[mid]);

            // If the user indicates that the application is running smoothly,
            // then move towards a more performant energy rate
            // else move towards a more efficient energy rate
            if (isApplicationRunningSmoothly)
            {
                _left = mid + 1;
            }
            else
            {
                _right = mid - 1;
            }
            await Task.Delay(TimeSpan.FromMinutes(delayInMinutes));
            PersonaNotification.SmoothnessCheckNotification(_executable, ++smoothnessCheckCount);
        }
        else
        {
            // If the user indicates that the application is running smoothly,
            // then notify user that auto-configuration was success
            // else notify user that auto-configuration failed
            if (isApplicationRunningSmoothly)
            {
                PersonaNotification.AutoConfigurationSuccessNotification(_executable);
            }
            else
            {
                PersonaNotification.FailedAutoConfigurationNotification(_executable);
            }
            smoothnessCheckCount = 0;
        }
    }
}