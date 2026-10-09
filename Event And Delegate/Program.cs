namespace Event_And_Delegate
{
    public class Program
    {
        static void Main(string[] args)
        {
            PlcDevice plcDevice = new PlcDevice();
            EventHandler<DataReceivedEventArgs> handler = null;
            int count = 0;
            handler = (sender, e) =>
            {
                count++;
                Console.WriteLine($"回调收到第{count}条｜时间:{e.Time}，值:{e.Value}");
                if (count == 3)
                {
                    Console.WriteLine($"总共收到{count}条");
                    plcDevice.DataReceived -= handler;
                    Console.WriteLine("取消订阅");
                }
            };
            plcDevice.DataReceived += handler;

            Console.WriteLine("PLC开始采集");
            plcDevice.Run();
            Console.WriteLine("PLC采集全部结束");



            PlcDevice plcDevice2 = new PlcDevice();
            EventHandler<DataReceivedEventArgs> handler2 = null;
            int count2 = 0;
            plcDevice2.Run2((value) =>
            {
                count2++;
                if (count2 > 3) return;
                Console.WriteLine($"收到数据:值是:{value}");
                if (count2 == 3) 
                {
                   Console.WriteLine($"总共收到{count2}条");
                    Console.WriteLine("取消订阅");
                }
            });
        }
    }
}
