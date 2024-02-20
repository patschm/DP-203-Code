namespace IOTBase;
public class DeviceSettings
{
    public Type? RealType { get => System.Type.GetType(Type); }
    public string Type { get; set; } = "System.Type";
    public RegistrationParameters? Parameters { get; set; }
}

