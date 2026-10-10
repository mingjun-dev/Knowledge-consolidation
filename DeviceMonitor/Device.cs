using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeviceMonitor
{
   public class Device
    {
        public string Name { get; set; }
        public string IpAddress { get; set; }
        public bool IsConnected { get; set; }
    }
}
