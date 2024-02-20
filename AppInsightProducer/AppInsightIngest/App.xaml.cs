using AppInsightData;
using Microsoft.Extensions.DependencyInjection;
using System;
using Microsoft.Extensions.Logging;
using System.Windows;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.Configuration;
using System.IO;
using AppInsightIngest.ViewModels;
using Microsoft.ApplicationInsights.Extensibility.PerfCounterCollector.QuickPulse;
using Microsoft.ApplicationInsights.WorkerService;
using Microsoft.Extensions.Logging.ApplicationInsights;
using Microsoft.ApplicationInsights;

namespace AppInsightIngest
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    { 
        private readonly IServiceProvider _serviceProvider;

        public IConfiguration Configuration { get; private set; }

        public App()
        {
            ConfigureHost();            
            var svcCollection = new ServiceCollection();
            ConfigureServices(svcCollection);
            _serviceProvider = svcCollection.BuildServiceProvider();
        }

        private void ConfigureHost()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
            Configuration = builder.Build();
        }

        private void ConfigureServices(IServiceCollection services)
        {
            var configuration = new TelemetryConfiguration();
            configuration.ConnectionString = Configuration.GetConnectionString("AppInsightConnectionString");
            LiveMetricsSignUp(configuration);
            var telemetryClient = new TelemetryClient(configuration);
            var logger = new ApplicationInsightsLogger("Requester", telemetryClient, new ApplicationInsightsLoggerOptions());

            services.AddSingleton<ILogger>(logger);
            services.AddSingleton<WebRequestSimulator>();
            services.AddSingleton<MainViewModel>();
            services.AddSingleton<MainWindow>();
        }

        private void OnStartUp(object sender, StartupEventArgs args)
        {
            var mainWindow = _serviceProvider.GetService<MainWindow>();
            mainWindow.Show();
        }
        private void LiveMetricsSignUp(TelemetryConfiguration configuration)
        {
            QuickPulseTelemetryProcessor processor = null;
            configuration.TelemetryProcessorChainBuilder.Use((next) => {
                processor = new QuickPulseTelemetryProcessor(next);
                return processor;
            }).Build();

            var QuickPulse = new QuickPulseTelemetryModule();
            QuickPulse.Initialize(configuration);
            QuickPulse.RegisterTelemetryProcessor(processor);
        }
    }
}
