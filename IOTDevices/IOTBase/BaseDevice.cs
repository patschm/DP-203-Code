using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.Azure.Devices.Client;

namespace IOTBase;
public abstract class BaseDevice : IDevice, INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;
    private readonly Timer _timer;
    private readonly RegistrationParameters _parameters;
    private DeviceClient? _client = null;
    private Random _random = new Random();
    private int usageCounter = 0;
    private bool _isFaulted = false;

    public string DeviceID { get; set; } = Guid.NewGuid().ToString().ToLower();
    public string? GroupName { get => _parameters.GroupName; }
    public bool IsFaulted
    {
        get
        {
            if (_isFaulted) return true;
            if (usageCounter > 100)
            {
                var rnd = _random.Next(0, 100);
                if (rnd <= BreakDownPercentage)
                {
                    _isFaulted = true;
                }
            }
            return false;
        }
    }
    public int BreakDownPercentage { get; set; } = 2;
    protected void NotifyChanged([CallerMemberName] string? property = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
    }
    public abstract void Update();
    public abstract Task SendAsync();

    protected async Task SendMessageAsync(Message message)
    {
        if (message == null || _client == null) return;
        message.MessageSchema = GroupName;
        try
        {
            await _client.SendEventAsync(message);
        }
        catch
        {
            throw;
        }
    }
    public async Task RegisterAsync()
    {
        _parameters.DeviceID = DeviceID;
        var regClient = new IOTRegistrationClient(_parameters);
        try
        {
            _client = await regClient.RegisterAsync();
        }
        catch
        {
            throw;
        }
    }
    public async Task StartAsync()
    {
        if (_client == null)
        {
            try
            {
                await RegisterAsync();
            }
            catch
            {
                throw;
            }
        }
    }
    private async void TimerTaskAsync(object? o)
    {
        usageCounter++;
        try
        {
            await SendAsync();
        }
        catch
        {
            throw;
        }
    }
    public async Task StopAsync()
    {
        await _timer.DisposeAsync();
        if (_client != null)
            await _client.DisposeAsync();

    }
    public BaseDevice(RegistrationParameters parameters)
    {
        _parameters = parameters;
        _timer = new Timer(TimerTaskAsync, period: 2000, dueTime: 0, state: null);
    }
}