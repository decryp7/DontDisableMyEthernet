using System;

namespace DontDisableMyEthernet.Core.NetworkAdapter
{
    public interface INetworkAdapter : IDisposable
    {
        string Name { get; }

        string NetworkConnectionID { get; }
        
        bool Enabled { get; }

        void Enable();

        void Refresh();
    }
}