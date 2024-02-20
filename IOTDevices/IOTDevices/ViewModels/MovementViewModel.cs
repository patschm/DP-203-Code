using IOTBase;

namespace IOTDevices.ViewModels;
public class MovementViewModel : SensorViewModel<MovementSensor>
{
    public MovementViewModel(ISensorFactory factory) : base(factory)
    {

    }
}

