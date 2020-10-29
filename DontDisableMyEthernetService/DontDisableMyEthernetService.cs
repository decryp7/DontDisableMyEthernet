using DontDisableMyEthernet.Core.NetworkAdapter;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace DontDisableMyEthernetService
{
    public partial class DontDisableMyEthernetService : ServiceBase
    {
        public DontDisableMyEthernetService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            if (!NetworkAdaptersService.Instance.Start())
            {
                Stop();
            };
        }

        protected override void OnStop()
        {
            NetworkAdaptersService.Instance.Stop();
        }
    }
}
