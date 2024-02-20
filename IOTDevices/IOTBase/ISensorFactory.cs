namespace IOTBase;
public interface ISensorFactory
{
    T? Create<T>() where T : BaseDevice;
}

