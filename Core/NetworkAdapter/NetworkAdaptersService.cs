using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Threading;
using DontDisableMyEthernet.Core.Logging;

namespace DontDisableMyEthernet.Core.NetworkAdapter
{
    public class NetworkAdaptersService : INetworkAdaptersService
    {
        //Settings
        //1 minute
        private int checkIntervalMinutes = 5;

        private static readonly Lazy<INetworkAdaptersService> lazy = new Lazy<INetworkAdaptersService>(() => new NetworkAdaptersService());

        public static INetworkAdaptersService Instance => lazy.Value;

        private readonly IList<INetworkAdapter> enabledNetworkAdapters = new List<INetworkAdapter>();
        private int busy;
        private Timer checkNetworkAdapterStatusTimer;
        private DateTime lastCheckDateTime = DateTime.MinValue;

        public bool Start()
        {
            try
            {
                SelectQuery wmiQuery =
                    new SelectQuery("SELECT * FROM Win32_NetworkAdapter WHERE NetConnectionId != NULL");
                ManagementObjectSearcher searchProcedure = new ManagementObjectSearcher(wmiQuery);
                foreach (ManagementObject item in searchProcedure.Get())
                {
                    INetworkAdapter networkAdapter = new NetworkAdapter(item);
                    if (networkAdapter.Enabled)
                    {
                        enabledNetworkAdapters.Add(networkAdapter);
                    }
                }
            }
            catch (Exception ex)
            {
                EventLogLogger.Instance.Write(
                    FormattableString.Invariant($"Unable to get enabled Network Adapters. {ex}"), LogLevel.Error);
            }

            if (!enabledNetworkAdapters.Any())
            {
                EventLogLogger.Instance.Write("There are no Network Adapters to monitor.");
                return false;
            }

            if (checkNetworkAdapterStatusTimer != null)
            {
                return true;
            }

            string networkAdapterNames = string.Join(Environment.NewLine, 
                enabledNetworkAdapters
                    .Select(a => FormattableString.Invariant($"{a.NetworkConnectionID} ({a.Name})")));

            EventLogLogger.Instance.Write(
                FormattableString
                    .Invariant($"Monitoring the following Network Adapters, {Environment.NewLine}{networkAdapterNames}"));

            checkNetworkAdapterStatusTimer = new Timer(CheckNetworkAdapterStatus, null, 0, checkIntervalMinutes * 60 * 1000);

            return true;
        }

        private void CheckNetworkAdapterStatus(object state)
        {
            if (Interlocked.CompareExchange(ref busy, 1, 0) == 1)
            {
                if (lastCheckDateTime != DateTime.MinValue &&
                    DateTime.Now - lastCheckDateTime > TimeSpan.FromMinutes(checkIntervalMinutes))
                {
                    EventLogLogger.Instance.Write(FormattableString.Invariant(
                        $"Seems that something is wrong. Last check is more then {checkIntervalMinutes} minutes ago. Last check: {lastCheckDateTime}"));
                }

                return;
            }

            try
            {
                lastCheckDateTime = DateTime.Now;

                string networkAdapterNames = string.Join(Environment.NewLine, 
                    enabledNetworkAdapters
                        .Select(a => FormattableString.Invariant($"{a.NetworkConnectionID} ({a.Name})")));

                EventLogLogger.Instance.Write(
                    FormattableString
                        .Invariant($"Checking the following Network Adapters to see if they are disabled, {Environment.NewLine}{networkAdapterNames}"));

                foreach (INetworkAdapter networkAdapter in enabledNetworkAdapters)
                {
                    networkAdapter.Refresh();

                    if (networkAdapter.Enabled)
                    {
                        continue;
                    }

                    EventLogLogger.Instance.Write(
                        FormattableString.Invariant(
                            $"Detected that Network Adapter {networkAdapter.NetworkConnectionID} ({networkAdapter.Name}) is Disabled. Enabling it..."));
                    networkAdapter.Enable();
                }
            }
            finally
            {
                Interlocked.Exchange(ref busy, 0);
            }
        }

        public void Stop()
        {
            checkNetworkAdapterStatusTimer?.Dispose();
            checkNetworkAdapterStatusTimer = null;

            foreach (INetworkAdapter enabledNetworkAdapter in enabledNetworkAdapters)
            {
                enabledNetworkAdapter.Dispose();
            }

            enabledNetworkAdapters.Clear();
        }
    }
}