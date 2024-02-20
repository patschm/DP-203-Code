using IOTBase;

namespace IOTDevices.Infra;

public class IOTSettings
{
    private List<DeviceSettings> _devices = new List<DeviceSettings>();
    public List<DeviceSettings> Devices 
    { 
        get
        {
            foreach(var device in _devices)
            {
                if (device == null || device.Parameters == null) continue;
                device.Parameters.IDScope = IDScope;
                device.Parameters.EndPoint = EndPoint!;
            }
            return _devices;
        }
        set
        {
            _devices = value;
        }
    }
    public string? IDScope { get; set; }
    public string? EndPoint { get; set; }

    
}