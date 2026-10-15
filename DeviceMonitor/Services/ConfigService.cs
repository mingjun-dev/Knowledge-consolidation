using DeviceMonitor.Models;
using Microsoft.Extensions.Configuration;
using OpenTK.Audio.OpenAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeviceMonitor.Services
{
    public class ConfigService:IConfigService
    {
        // 依赖底层配置对象，通过构造函数注入进来
        private readonly IConfiguration _configuration;

        /// <summary>
        /// 构造函数注入：DI容器会自动传入IConfiguration实例（第2步注册过的）
        /// </summary>
        public ConfigService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public PlcOption GetPlcConfig()
        {
            PlcOption option = new PlcOption();
            _configuration.GetSection("Plc").Bind(option);
            return option;
        }

        public DbOption GetDbConfig()
        {
            DbOption option = new DbOption();
            _configuration.GetSection("Database").Bind(option);
            return option;
        }
    }
}
