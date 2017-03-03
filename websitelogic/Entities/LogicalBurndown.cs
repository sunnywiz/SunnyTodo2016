using System;
using System.Collections.Generic;

namespace websitelogic.Entities
{
    public class LogicalBurndown
    {
        public Guid BurndownID { get; set; }
        public Guid OwnerUserId { get; set; }
        public List<String> Definition { get; set; }
        public List<Tuple<DateTime,String>> History { get; set; }
    }
}