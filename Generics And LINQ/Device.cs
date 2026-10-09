using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Generics_And_LINQ
{
    public class Device
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public bool IsConnected { get; set; }
        public double Temperature { get; set; }
        public Device(string name, string type, bool isConnected, double temperature)
        {
            Name = name;
            Type = type;
            IsConnected = isConnected;
            Temperature = temperature;
        }
        public Device()
        {
                
        }
    }
}
