using FreeSql.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeviceMonitor.Models
{
    [Table(Name = "SensorRecord")]
    public class SensorRecord
    {
        [Column(IsPrimary = true,IsIdentity =true)]
        public long Id { get; set; }

        [Column(StringLength =64,IsNullable =true)]
        public string Name { get; set; }

        [Column(DbType ="REAL",IsNullable =false)]
        public double Temperature {  get; set; }

        [Column(IsNullable =false)]
        public DateTime RecordTime { get; set; }
    }
}
