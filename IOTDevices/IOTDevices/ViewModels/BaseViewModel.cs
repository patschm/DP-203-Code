using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace IOTDevices.ViewModels;
public class BaseViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    public void PropertyChange([CallerMemberName]string? property = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
    }
}

