using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeviceMonitor.Models
{
    public class PlcOption
    {
        public string IpAddress { get; set; }
        public int Port { get; set; }
        public int ReadIntervalMs { get; set; }
        public double TemperatureScale { get; set; }
    }
}
