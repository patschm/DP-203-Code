using Microsoft.Extensions.DependencyInjection;

namespace IOTDevices.Infra;
public static class ViewModelLocator
{
    private static ServiceCollection _services = new ServiceCollection();
    private static IServiceProvider? _provider = null;
    
    public static IServiceCollection Services { get=>_services;}    
    public static T? GetViewModel<T>()
    { 
        if (_provider == null)
        {
            _provider = _services.BuildServiceProvider(); 
        }
        return _provider.GetService<T>();
    }

}

