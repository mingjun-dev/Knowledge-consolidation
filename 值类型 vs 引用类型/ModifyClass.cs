using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace 值类型_vs_引用类型
{
    public static class ModifyClass
    {
        public static string Modify_Class(DeviceInfo d) 
        {
          return  d.Name = "修改后的名字-----------张三";
        }

        public static string ModifyStruct(DeviceStruct d)
        {
           return d.Name =  "修改后的名字------------李四";
        }
    }
}
