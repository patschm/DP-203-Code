using IOTBase;

namespace IOTDevices.ViewModels;
public class PressureViewModel : SensorViewModel<PressureSensor>
{
    public PressureViewModel(ISensorFactory factory) : base(factory)
    {

    }
}

