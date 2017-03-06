using System;
using System.Data.Entity;

namespace WebApplication1.Models
{
    public class BurndownDbContext : DbContext
    {
        public BurndownDbContext() : base("BurndownDbConnection")
        {
            Database.SetInitializer(new DropCreateDatabaseIfModelChanges<BurndownDbContext>());
        }
        public virtual DbSet<DbBurndown> Burndowns { get; set; }

        public class DbBurndown
        {
            public Guid Id { get; set; }
            public string Definition { get; set; }
        }
    }
}