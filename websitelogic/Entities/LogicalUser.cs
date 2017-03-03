using System;

namespace websitelogic.Entities
{
    public class LogicalUser
    {
        public static readonly Guid AnonymousUserId = Guid.Empty;

        public Guid UserId { get; set; }
    }
}
