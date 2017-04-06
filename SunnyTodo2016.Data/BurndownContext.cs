using System.Data.Entity;

namespace SunnyTodo2016.Data
{
    public class BurndownContext : DbContext
    {
        public BurndownContext() : base("SunnyTodo2016")
        {
            Database.SetInitializer(new CreateDatabaseIfNotExists<BurndownContext>());
        }

        public virtual DbSet<Burndown> Burndowns { get; set; }
        public virtual DbSet<HistoryLine> History { get; set; }
    }
}