using System.Windows.Controls;
using IOTDevices.Infra;
using IOTDevices.ViewModels;

namespace IOTDevices;
/// <summary>
/// Interaction logic for WeatherSensorView.xaml
/// </summary>
public partial class WeatherSensorView : UserControl
{
    public WeatherSensorView()
    {
        InitializeComponent();
        DataContext = ViewModelLocator.GetViewModel<WeatherViewModel>();
    }
}

