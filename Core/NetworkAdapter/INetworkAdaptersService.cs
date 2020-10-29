namespace DontDisableMyEthernet.Core.NetworkAdapter
{
    public interface INetworkAdaptersService
    {
        bool Start();

        void Stop();
    }
}