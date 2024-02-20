using IOTBase;
using IOTDevices.Infra;
using IOTDevices.ViewModels;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.IO;
using System.Windows;

namespace IOTDevices;
/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    public IConfiguration? Configuration { get; private set; }

    public App()
    {
        ConfigureHost();
        
    }

    private void ConfigureHost()
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
        Configuration = builder.Build();
    }

    private void ConfigureServices()
    {
        var iot = Configuration?.GetSection("IOTSettings").Get<IOTSettings>();
        if (iot == null) return;
        var factory = new SensorFactory(iot.Devices);
        ViewModelLocator.Services.AddSingleton<ISensorFactory>(factory);
        ViewModelLocator.Services.AddSingleton<MainWindow>();
        ViewModelLocator.Services.AddTransient<MovementViewModel>();
        ViewModelLocator.Services.AddTransient<PressureViewModel>();
        ViewModelLocator.Services.AddTransient<WeatherViewModel>();
    }

    private void OnStartUp(object sender, StartupEventArgs args)
    {
        ConfigureServices();
        var mainWindow = ViewModelLocator.GetViewModel<MainWindow>();
        if (mainWindow != null)
        {
            mainWindow.Show();
        }
    }

}

