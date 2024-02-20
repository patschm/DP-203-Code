using System.Collections.ObjectModel;
using System.Windows.Input;
using IOTBase;
namespace IOTDevices.ViewModels;
public abstract class SensorViewModel<T> : BaseViewModel where T : BaseDevice
{
    private ObservableCollection<T> _sensors = new ObservableCollection<T>();
    private uint _nrOfSensors = 0;
    private bool _isSending = false;

    protected ISensorFactory SensorFactory { get; private set; }

    public SensorViewModel(ISensorFactory factory)
    {
        SensorFactory = factory;
    }

    public ObservableCollection<T> Sensors { get => _sensors; }
    public uint NrOfSensors
    {
        get
        {
            return _nrOfSensors;
        }
        set
        {
            _nrOfSensors = value;
            PropertyChange();
        }
    }

    public ICommand SetDevicesCommand { get => new RelayCommand(async o => await ConfigureSensorsAsync(), o => !_isSending); }

    protected async Task ConfigureSensorsAsync()
    {
        _isSending = true;
        var deltaSensors = Sensors.Count - NrOfSensors;
        if (deltaSensors > 0)
        {
            // Remove Sensors
            while (deltaSensors > 0)
            {
                var sensor = Sensors.Last();
                await sensor.StopAsync();
                Sensors.Remove(sensor);
            }
        }
        else if (deltaSensors < 0)
        {
            // Add Sensors
            for (int i = 0; i < Math.Abs(deltaSensors); i++)
            {
                var sensor = SensorFactory.Create<T>();
                if (sensor != null)
                {
                    await sensor.StartAsync();
                    Sensors.Add(sensor);
                }
            }
        }
        _isSending = false;
    }
}

