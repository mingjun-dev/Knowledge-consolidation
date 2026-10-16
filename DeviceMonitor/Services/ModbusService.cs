using CommunityToolkit.Mvvm.Messaging;
using DeviceMonitor.Models;
using Modbus.Device;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DeviceMonitor.Services
{
    public class ModbusService
    {
        private TcpClient? _tcpClient;
        private IModbusMaster _master;
        public async Task ConnectAsync(string ip, int port,int timeoutMs)
        {
            Disconnect();
            try
            {
                _tcpClient = new TcpClient();
                using var cts = new CancellationTokenSource(3000);
                await _tcpClient.ConnectAsync(ip, port, cts.Token);
                _master = ModbusIpMaster.CreateIp(_tcpClient);
                _master.Transport.ReadTimeout = 5000;
                _master.Transport.WriteTimeout = 5000;
            }
            catch (Exception ex)
            {
                WeakReferenceMessenger.Default.Send(new Message.DeviceStatusMessage("", false, DateTime.Now, $"通讯连接失败:{ex.Message}"));
                Disconnect();
                throw;
            }
           
        }
        public async Task<ushort[]> ReadHoldingRegistersAsync(byte slaveId, ushort startAddress, ushort count) 
        {

            if (_master is null) 
            {
                throw new InvalidOperationException("Modbus为建立连接");
            }
            for (int i = 0; i < 3; i++)
            {
                try
                {
                    return await _master!.ReadHoldingRegistersAsync(slaveId, startAddress, count);
                }
                catch(Exception ex)
                {
                    if (i == 2) throw;
                    await Task.Delay(200);
                    WeakReferenceMessenger.Default.Send(new Message.DeviceStatusMessage("", false, DateTime.Now, $"读取失败:{ex.Message}"));
                }
            }
            return Array.Empty<ushort>();
        }

        public async Task WriteSingleRegisterAsync(byte slaveId, ushort address, ushort value) 
        {
            if (_master is null)
            {
                throw new InvalidOperationException("Modbus为建立连接");
            }
            for (int i = 0; i < 3; i++)
            {
                try
                {
                    await _master!.WriteSingleRegisterAsync(slaveId, address, value);
                    return;
                }
                catch(Exception ex)
                {
                    if (i == 2) throw;
                    await Task.Delay(200);
                    WeakReferenceMessenger.Default.Send(new Message.DeviceStatusMessage("", false, DateTime.Now, $"写入失败:{ex.Message}"));
                }
            }
        }

        public async Task<double> ReadTemperatureAsync(byte slaveId, ushort address) 
        {
            var raw = await ReadHoldingRegistersAsync(slaveId, address, 1);
            if (raw == null || raw.Length == 0)
                throw new Exception("Modbus读取寄存器返回空数据");
            return raw[0] / 10.0;
        }
        public void Disconnect()
        {
            _master = null;
            if (_tcpClient != null)
            {
                try
                {
                    _tcpClient.Close();
                }
                catch (Exception ex)
                {

                }
                _tcpClient.Dispose();
                _tcpClient = null;
            }
        }

    }
}
