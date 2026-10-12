using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeviceMonitor.Services
{
    public class MockDeviceService : IDeviceService
    {
      
        private Random _random = new Random();
        public MockDeviceService()
        {
            
            
        }
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
        Random _rand = new Random();
        public async Task<double> ReadTemperatureAsync(string device, CancellationToken ct)
        {
            var delay = _rand.Next(300, 1500);
            await Task.Delay(delay, ct);
            return _random.NextDouble() * (85 - 25) + 25;
        }
    }
}
