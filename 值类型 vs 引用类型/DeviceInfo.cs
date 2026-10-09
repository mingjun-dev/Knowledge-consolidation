using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace 值类型_vs_引用类型
{
    public class DeviceInfo
    {
        public string Name  { get; set; }
        public string Status { get; set; }
    }

    public struct DeviceStruct 
    {
        public string Name { get; set; }
        public string Status { get; set; }
    }
}
