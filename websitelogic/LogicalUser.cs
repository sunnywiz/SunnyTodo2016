using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace websitelogic
{
    public class LogicalUser
    {
        public static readonly Guid AnonymousUserId = Guid.Empty;

        public Guid UserId { get; set; }
    }
}
