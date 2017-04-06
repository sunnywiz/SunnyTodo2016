using System;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SunnyTodo2016.Data
{
    public class Burndown
    {
        public Guid BurndownId { get; set; }

        public string Title { get; set; }

        public string Definition { get; set; }

        public Guid OwnerUserId { get; set; }

        public virtual DbSet<HistoryLine> History { get; set; }
    }

    public class HistoryLine
    {
        public Guid HistoryLineId { get; set; }
        public DateTime DateTime { get; set; }
        public string TaskLine { get; set; }
        public Guid BurndownId { get; set; }
        public virtual Burndown Burndown { get; set; }
    }
}
