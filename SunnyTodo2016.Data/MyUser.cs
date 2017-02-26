using System;
using System.Collections.Generic;

namespace SunnyTodo2016.Data
{
    public class MyUser
    {
        public Guid AnonymousUserId = Guid.Empty; 

        public Guid MyUserID { get; set; }

        public virtual ICollection<Burndown> Burndowns { get; set; }
        
    }
}