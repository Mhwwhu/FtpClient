using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.Database.Entity
{
    public class RecordEntity
    {
        public int Id { get; set; }
        public DateTime Timestamp { get; set; }
        public int? ServerId { get; set; }
        public string Command { get; set; }
    }
}
