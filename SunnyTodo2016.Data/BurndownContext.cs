using System.Data.Entity;

namespace SunnyTodo2016.Data
{
    public class BurndownContext : DbContext
    {
        public BurndownContext() : base("SunnyTodo2016")
        {
#if DEBUG
            Database.SetInitializer(new DropCreateDatabaseIfModelChanges<BurndownContext>());
#endif
            // If in release mode, use schema compare to update production database. 
            // Not using migrations. 
        }

        public virtual DbSet<Burndown> Burndowns { get; set; }
        public virtual DbSet<HistoryLine> History { get; set; }
    }
}