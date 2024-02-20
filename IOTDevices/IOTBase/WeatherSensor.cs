using System.Text;
using Microsoft.Azure.Devices.Client;
using Newtonsoft.Json;

namespace IOTBase;
public class WeatherSensor : BaseDevice
{
    private readonly Random _rnd;
    private double _temperature = 25F;
    private double _humidity = 50F;

    public double Temperature { get => _temperature; private set { _temperature = value; NotifyChanged(); } }
    public double Humidity { get => _humidity; private set { _humidity = value; NotifyChanged(); } }

    public override void Update()
    {
        var incTemp = _rnd.Next(-100, 100);
        var incHumid = _rnd.Next(-100, 100);
        if (IsFaulted)
        {
            incTemp = _rnd.Next(0, 1000);
            incHumid = _rnd.Next(0, 400);
        }

        _temperature += incTemp / 10;
        _humidity += incHumid / 20;
        Temperature = _temperature < 0 ? 0 : _temperature;
        Humidity = _humidity < 0 ? 0 : _humidity;
        Humidity = _humidity > 100 ? 100 : _humidity;
    }
    public override async Task SendAsync()
    {
        Update();
        var msg = CreateMessage();
        await SendMessageAsync(msg);
    }

    private Message CreateMessage()
    {

        var body = JsonConvert.SerializeObject(new
        {
            temperature = Temperature,
            humidity = Humidity
        });

        var msg = new Message(Encoding.UTF8.GetBytes(body))
        {
            ContentType = "application/json",
            ContentEncoding = "utf-8"
        };

        msg.Properties.Add("temperatureAlert", (Temperature > 50) ? "true" : "false");
        msg.Properties.Add("humidityAlert", (Humidity > 80) ? "true" : "false");

        return msg;
    }

    public WeatherSensor(RegistrationParameters parameters) : base(parameters)
    {
        _rnd = new Random();
    }
}

