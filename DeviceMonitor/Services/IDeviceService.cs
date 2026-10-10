using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeviceMonitor.Services
{
    public interface IDeviceService
    {
        public Task<List<Device>> GetDevicesAsync();
    }

    public class MockDeviceService : IDeviceService
    {
        public async Task<List<Device>> GetDevicesAsync()
        {
            await Task.Delay(200);
            return new List<Device>()
        {
            new Device
            {
                Name = "PLC‑001",
                IpAddress = "192.168.1.101",
                IsConnected = false
            },
            new Device
            {
                Name = "PLC‑002",
                IpAddress = "192.168.1.102",
                IsConnected = false
            },
            new Device
            {
                Name = "Robot‑001",
                IpAddress = "192.168.1.201",
                IsConnected = false
            },
            new Device
            {
                Name = "Robot‑002",
                IpAddress = "192.168.1.202",
                IsConnected = false
            },
            new Device
            {
                Name = "Camera‑001",
                IpAddress = "192.168.1.301",
                IsConnected = false
            } };
        }
    }
}

