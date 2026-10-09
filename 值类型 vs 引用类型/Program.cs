using System.Xml.Linq;

namespace 值类型_vs_引用类型
{
    public class Program
    {
        static void Main(string[] args)
        {
           
            DeviceInfo deviceInfo = new DeviceInfo();
            deviceInfo.Name = "1234";
            Console.WriteLine($"Class没有改之前--{deviceInfo.Name}");
            ModifyClass.Modify_Class(deviceInfo);
            Console.WriteLine($"Class改了之后---{deviceInfo.Name}");


            
            DeviceStruct deviceStruct = new DeviceStruct();
            deviceStruct.Name = "qwer";
            Console.WriteLine($"Struct没改之前---{deviceStruct.Name}");
            ModifyClass.ModifyStruct(deviceStruct);
            Console.WriteLine($"Struct改了之后---{deviceStruct.Name}");


            List<DeviceInfo> a = new List<DeviceInfo>();
            a.Add( new DeviceInfo { Name = "设备1"});
            var b = a;
            b.Add(new DeviceInfo {Name = "设备2"});
            Console.WriteLine($"a.Count: {a.Count}, b.Count: {b.Count}");

            var c = a[0];
            c.Name = "设备3";
            Console.WriteLine(a[0].Name);
        }
    }
}
