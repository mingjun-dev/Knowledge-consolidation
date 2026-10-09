using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace 抽象方法_虚方法_运行时多态和接口
{
    public class RobotDevice : DeviceBase
    {
        public override void Connect()
        {
            Console.WriteLine($"---------机器人{Name}连接------握手并回原点");
        }
    }
}
