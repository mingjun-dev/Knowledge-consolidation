using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Event_And_Delegate
{
    public class DataReceivedEventArgs:EventArgs
    {
        public DateTime Time { get; set; }
        public double Value { get; set; }
    }
}
