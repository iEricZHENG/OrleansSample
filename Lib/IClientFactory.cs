using Orleans;

namespace Lib
{
    public interface IClientFactory
    {
        IClusterClient GetClient();
    }
}
