using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeviceMonitor.Models
{
    public static class Message
    {
        public record DeviceStatusMessage(string DeviceName, bool IsConnected, DateTime Time);
    }
}
