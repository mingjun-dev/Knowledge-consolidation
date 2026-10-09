using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace 抽象方法_虚方法_运行时多态和接口
{
    public class PlcDevice : DeviceBase,IConfigurable
    {
        public override void Connect()
        {
            Console.WriteLine($"-------PLC{Name}---建立 Modbus 连接");
        }
        public override void DisConnect()
        {
            Console.WriteLine($"-------PLC{Name}---断开连接");
        }

        public void LoadConfig(string path)
        {
            Console.WriteLine($"----------PLC{Name}-------从路径 {path} 加载配置文件");
        }
    }
}
