namespace Generics_And_LINQ
{
    public class Program
    {
        static void Main(string[] args)
        {
            List<Device> deviceList = new List<Device>();
            
            deviceList.Add(new Device()
            {
                Name = "PLC-01",
                Type = "PLC",
                IsConnected = true,
                Temperature = 42.5
            });
            deviceList.Add(new Device()
            {
                Name = "Robot-01",
                Type = "Robot",
                IsConnected = true,
                Temperature = 38.2
            });
            deviceList.Add(new Device()
            {
                Name = "Sensor-01",
                Type = "Sensor",
                IsConnected = false,
                Temperature = 22.1
            });
            deviceList.Add(new Device()
            {
                Name = "PLC-02",
                Type = "PLC",
                IsConnected = false,
                Temperature = 51.8
            });
            deviceList.Add(new Device()
            {
                Name = "Robot-02",
                Type = "Robot",
                IsConnected = true,
                Temperature = 44.7
            });
            deviceList.Add(new Device()
            {
                Name = "Sensor-02",
                Type = "Sensor",
                IsConnected = true,
                Temperature = 26.4
            });
            deviceList.Add(new Device()
            {
                Name = "PLC-03",
                Type = "PLC",
                IsConnected = true,
                Temperature = 39.6
            });
            deviceList.Add(new Device()
            {
                Name = "Robot-03",
                Type = "Robot",
                IsConnected = false,
                Temperature = 55.2
            });
            deviceList.Add(new Device()
            {
                Name = "Sensor-03",
                Type = "Sensor",
                IsConnected = true,
                Temperature = 24.9
            });
            var connect = deviceList.Where(d => d.IsConnected == true);
            Console.WriteLine("Connected Devices:");
            foreach (var device in connect)
            {
                Console.WriteLine($"连接的设备名称有:- {device.Name}---类型是: ({device.Type})");
            }

            var name = deviceList.Select(d => d.Name).OrderBy(n => n);
            Console.WriteLine("按名称升序排序为:");
            foreach (var device in name) 
            {
                Console.WriteLine(device);
            }

            var type = deviceList.GroupBy(d => d.Type).Select(g => new { Type = g.Key, Count = g.Count() });
            Console.WriteLine("按类型分组统计:");
            foreach (var group in type)
            {
                Console.WriteLine($"类型: {group.Type}, 设备数量: {group.Count}");
            }
            var temperature =
                deviceList.OrderByDescending(d => d.Temperature)
                .Take(3);//拿取前几个元素
            Console.WriteLine("温度最高的三台设备为:");
            foreach (var item in temperature) 
            {
                Console.WriteLine($"设备名称: {item.Name}, 类型: {item.Type}, 温度: {item.Temperature}");
            }
            var averageTemperature = deviceList.Where(d =>d.IsConnected).Average(d=>d.Temperature);
            Console.WriteLine($"所有连接设备的平均温度为: {averageTemperature:F2}");

            var any = deviceList.Any(d => d.Temperature >80);
            if (any) 
            {
                Console.WriteLine("有高温设备");
            }
            else
            {
                Console.WriteLine("没有高温设备");
            }

            var firstName = deviceList.FirstOrDefault(d => d.Name == "PLC-01");
            Console.WriteLine(firstName?.Name);//找到了返回对象

            var firstName2 = deviceList.FirstOrDefault(d => d.Name == "PLC-011");
            Console.WriteLine(firstName2?.Name);//没找到返回null,可以加null条件运算符避免报错  


            var first = deviceList.First(d =>d.Name== "PLC-01");
            Console.WriteLine($"------------------------{first.Name}");//找到了返回对象

            var first2 = deviceList.First(d => d.Name == "PLC-0001");
            Console.WriteLine($"------------------------{first2.Name}");//没找到返回异常,报错

            var type2 = from d in deviceList
                        group d by d.Type into g
                        select new { Type = g.Key, Count = g.Count() };
            foreach (var item in type2)
            {
                Console.WriteLine($"类型:{item.Type}, 数量:{item.Count}");
            }
        }
    }
}
