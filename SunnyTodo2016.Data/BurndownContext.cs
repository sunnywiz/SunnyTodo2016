using System.Data.Entity;

namespace SunnyTodo2016.Data
{
    public class BurndownContext : DbContext
    {
        public BurndownContext() : base("SunnyTodo2016")
        {
            Database.SetInitializer(new DropCreateDatabaseIfModelChanges<BurndownContext>());
        }

        public virtual DbSet<MyUser> MyUsers { get; set; }
        public virtual DbSet<Burndown> Burndowns { get; set; }
    }
}