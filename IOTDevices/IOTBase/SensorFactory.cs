namespace IOTBase;
public class SensorFactory : ISensorFactory
{
    private List<DeviceSettings> _devices;

    public SensorFactory(List<DeviceSettings> devices)
    {
        _devices = devices;
    }

    public T? Create<T>() where T : BaseDevice
    {
        var ds = _devices.Where(d => d.RealType == typeof(T)).FirstOrDefault();
        if (ds != null)
        {
            return (T?)Activator.CreateInstance(typeof(T), ds.Parameters);
        }
        return default;
    }
}

