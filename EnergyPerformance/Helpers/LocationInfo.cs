using System.ComponentModel;


namespace EnergyPerformance.Helpers;
public class LocationInfo : INotifyPropertyChanged
{
    private string _country;
    private string _postCode;
    private string _region;
    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    public string Country
    {
        get => _country;
        set
        {
            _country = value;
            OnPropertyChanged(nameof(Country));
        }
    }
    public string PostCode
    {
        get => _postCode;
        set
        {
            _postCode = value;
            OnPropertyChanged(nameof(PostCode));
        }
    }

    public string Region
    {
        get => _region;
        set
        {
            _region = value;
            OnPropertyChanged(nameof(Region));
        }
    }



    public LocationInfo()
    {
        Country = "Unknown";
        PostCode = "Unknown";
        Region = "Unknown";
    }
}
