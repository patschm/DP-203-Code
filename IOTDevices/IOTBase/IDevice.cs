namespace IOTBase;
public interface IDevice
{
    Task StartAsync();
    Task StopAsync();
    Task SendAsync();
    void Update();
    Task RegisterAsync();
    bool IsFaulted {get;}
}
