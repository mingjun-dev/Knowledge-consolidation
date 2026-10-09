using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace 抽象方法_虚方法_运行时多态和接口
{
    public abstract class DeviceBase
    {
        public string Name { get; set; }
        public string IpAddress { get; set; }
        public bool IsConnected { get; set; }


        public abstract void Connect();
        public virtual void DisConnect()
        {
            Console.WriteLine("DeviceBase--------断开连接");
            IsConnected = false;
          
        }

        public void Report() 
        {
            Console.WriteLine($"----------{Name}---------执行了Report");
            Connect();
            DisConnect();
        }
    }
}
