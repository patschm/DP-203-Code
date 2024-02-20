using System.Text;
using Microsoft.Azure.Devices.Client;
using Newtonsoft.Json;

namespace IOTBase;
public class PressureSensor : BaseDevice
{
    private readonly Random _rnd;
    private double _pressure = 1F;

    public double Pressure { get => _pressure; private set { _pressure = value; NotifyChanged(); } }
    public override void Update()
    {
        var deltaPressure = _rnd.Next(-100, 100);
        if (IsFaulted)
            deltaPressure = _rnd.Next(-200, -50);
        Pressure += deltaPressure / 50;
        Pressure = Pressure < 0 ? 0 : Pressure;
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
            pressure = Pressure
        });

        var msg = new Message(Encoding.UTF8.GetBytes(body))
        {
            ContentType = "application/json",
            ContentEncoding = "utf-8"
        };

        msg.Properties.Add("pressureAlert", (Pressure > 5) ? "true" : "false");

        return msg;
    }

    public PressureSensor(RegistrationParameters parameters) : base(parameters)
    {
        _rnd = new Random();
    }
}

