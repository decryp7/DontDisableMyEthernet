using System;
using System.Management;
using System.Threading;
using DontDisableMyEthernet.Core.Logging;

namespace DontDisableMyEthernet.Core.NetworkAdapter
{
    /// <summary>
    /// Refer to https://docs.microsoft.com/en-us/windows/win32/cimwin32prov/win32-networkadapter
    /// Wraps the Management object
    /// </summary>
    public class NetworkAdapter : INetworkAdapter
    {
        private readonly ManagementObject networkAdapterManagementObject;

        private const string NameProperty = "Name";
        private const string NetEnabledProperty = "NetEnabled";
        private const string NetConnectionIDProperty = "NetConnectionID";
        private const string EnableMethod = "Enable";
        private const string Unknown = "Unknown";

        public NetworkAdapter(ManagementObject managementObject)
        {
            networkAdapterManagementObject = managementObject ?? throw new ArgumentNullException(nameof(managementObject));
        }

        public string Name
        {
            get
            {
                try
                {
                    return networkAdapterManagementObject[NameProperty].ToString();
                }
                catch (Exception ex)
                {
                    EventLogLogger.Instance.Write(
                        FormattableString.Invariant($"Unable to get NetworkAdapter Name. {ex}"), LogLevel.Error);
                    return Unknown;
                }
            }
        }

        public string NetworkConnectionID
        {
            get
            {
                try
                {
                    return networkAdapterManagementObject[NetConnectionIDProperty].ToString();
                }
                catch (Exception ex)
                {
                    EventLogLogger.Instance.Write(
                        FormattableString.Invariant($"Unable to get NetworkAdapter NetworkConnectionID. {ex}"), LogLevel.Error);
                    return Unknown;
                }
            }
        }

        public bool Enabled
        {
            get
            {
                try
                {
                    return (bool) networkAdapterManagementObject[NetEnabledProperty];
                }
                catch (Exception ex)
                {
                    EventLogLogger.Instance.Write(
                        FormattableString.Invariant($"Unable to get NetworkAdapter({Name}) Enabled status. {ex}"), LogLevel.Error);
                    return false;
                }
            }
        }
        
        public void Enable()
        {
            try
            {
                networkAdapterManagementObject.InvokeMethod(EnableMethod, new object[]{});
            }
            catch (Exception ex)
            {
                EventLogLogger.Instance.Write(
                    FormattableString.Invariant($"Unable to enable NetworkAdapter({Name}). {ex}"), LogLevel.Error);
            }
        }

        public void Refresh()
        {
            try
            {
                Retry(() => networkAdapterManagementObject.Get());
            }
            catch (Exception ex)
            {
                EventLogLogger.Instance.Write(
                    FormattableString.Invariant($"Unable to refresh NetworkAdapter({Name}). {ex}"), LogLevel.Error);
            }
        }

        private void Retry(Action action)
        {
            bool failed = false;
            Exception exception = null;

            for (int i = 0; i < 3; i++)
            {
                failed = false;
                Thread.Sleep(500);
                
                try
                {
                    action.Invoke();
                }
                catch(Exception ex)
                {
                    //Ignore exception
                    exception = ex;
                    failed = true;
                }

                if (!failed)
                {
                    return;
                }
            }

            if (failed && 
                exception != null)
            {
                throw exception;
            }
        }

        public void Dispose()
        {
            networkAdapterManagementObject?.Dispose();
        }
    }
}