using IOTBase;

namespace IOTDevices.ViewModels;
public class WeatherViewModel : SensorViewModel<WeatherSensor>
{
    public WeatherViewModel(ISensorFactory factory) : base(factory)
    {

    }
}

