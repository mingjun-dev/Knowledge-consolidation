using DeviceMonitor.Models;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeviceMonitor.Services
{
    public static class SqliteDbService
    {
        public static IFreeSql freeSql { get; private set; }
        public static void Initialize(string connectionString, FreeSql.DataType freesql = FreeSql.DataType.Sqlite)
        {
            if (freeSql != null) return; //防止重复初始化

            freeSql = new FreeSql.FreeSqlBuilder()
                .UseConnectionString(freesql, connectionString)
                .UseAdoConnectionPool(true)
                .UseAutoSyncStructure(true)
                .Build();
        }
        public async static Task<long> InsertAsync(double Temperature, DateTime RecordTime)
        {
            var sr = new SensorRecord();
            sr.Temperature = Temperature;
            sr.RecordTime = RecordTime;
            long newId = await freeSql.Insert(sr).ExecuteIdentityAsync();
            return newId;
        }

        public async static Task<List<SensorRecord>> QuerytAsync(DateTime from, DateTime to)
        {
            var list =await freeSql.Select<SensorRecord>()
                  .Where(r => r.RecordTime >= from && r.RecordTime <= to)
                  .OrderBy(r => r.RecordTime)
                  .ToListAsync();
            return list;
        }
        public static async Task<List<SensorRecord>> QuerySensorLatestNAsync(int takeCount)
        {
            var list = await freeSql.Select<SensorRecord>()
                .OrderByDescending(r => r.RecordTime) 
                .Take(takeCount)                      
                .ToListAsync();
            return list;
        }
    }
}
