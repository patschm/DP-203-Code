using System.Text;
using Microsoft.Azure.Devices.Client;
using Newtonsoft.Json;

namespace IOTBase;
public class MovementSensor : BaseDevice
{
    private readonly Random _rnd;
    private int _movementPerMinute;
    public int Movement { get => _movementPerMinute; private set { _movementPerMinute = value; NotifyChanged(); } }

    public override void Update()
    {
        if (IsFaulted)
            Movement = _rnd.Next(700, 1000);
        else
            Movement = _rnd.Next(0, 1000);
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
            movementsPerMinute = Movement
        });

        var msg = new Message(Encoding.UTF8.GetBytes(body))
        {
            ContentType = "application/json",
            ContentEncoding = "utf-8"
        };

        msg.Properties.Add("tooBusyAlert", (Movement > 800) ? "true" : "false");

        return msg;
    }

    public MovementSensor(RegistrationParameters parameters) : base(parameters)
    {
        _rnd = new Random();
    }
}

