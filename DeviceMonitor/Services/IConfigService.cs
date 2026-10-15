using DeviceMonitor.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeviceMonitor.Services
{
    public interface IConfigService
    {
        PlcOption GetPlcConfig();
        DbOption GetDbConfig();
    }
}
