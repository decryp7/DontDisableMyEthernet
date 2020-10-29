using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DontDisableMyEthernet.Core.NetworkAdapter;

namespace DontDisableMyEthernet
{
    class Program
    {
        private static readonly ManualResetEvent waitEvent = new ManualResetEvent(false);
        private static Mutex mutex;

        static void Main(string[] args)
        {
            Process currentProcess = Process.GetCurrentProcess();

            string mutexName = string.Format(
                CultureInfo.InvariantCulture,
                @"Global\{0}",
                currentProcess.ProcessName);

            mutex = new Mutex(true, mutexName, out bool isOwner);

            if (!isOwner ||
                !NetworkAdaptersService.Instance.Start())
            {
                //Exit if there is another existing process running
                Environment.Exit(0);
            }

            //wait forever
            waitEvent.WaitOne();
        }
    }
}
