using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Event_And_Delegate
{
    public class PlcDevice
    {
        public event EventHandler<DataReceivedEventArgs> DataReceived;

        public void Run() 
        {
            Random random = new Random();

            for (int i = 0; i < 5; i++) 
            {
                Thread.Sleep(300);
                DataReceived?.Invoke(this,new DataReceivedEventArgs() { Time = DateTime.Now, Value =random.Next(0,100) });//事件回调
            }
            Console.WriteLine("回调函数全部执行完了");
            
        }
        public void Run2(Action<double> action)
        {
            Random random = new Random();

            for (int i = 0; i < 5; i++)
            {
                Thread.Sleep(300);
                action?.Invoke( random.Next(0, 100));//委托回调
            }
        }
    }
}
 