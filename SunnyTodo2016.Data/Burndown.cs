using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SunnyTodo2016.Data
{
    public class Burndown
    {
        public Guid BurndownID { get; set; }

        public string Title { get; set; }

        public string Definition { get; set; }

        public Guid OwnerUserID { get; set; }
    }
}
