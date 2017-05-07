namespace SunnyTodo2016.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Initial : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Burndowns",
                c => new
                    {
                        BurndownId = c.Guid(nullable: false),
                        Title = c.String(),
                        Definition = c.String(),
                        OwnerUserId = c.Guid(nullable: false),
                        CreatedDate = c.DateTime(nullable: false),
                        LastModifiedDate = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.BurndownId);
            
            CreateTable(
                "dbo.HistoryLines",
                c => new
                    {
                        HistoryLineId = c.Guid(nullable: false),
                        DateTime = c.DateTime(nullable: false),
                        TaskLine = c.String(),
                        BurndownId = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.HistoryLineId)
                .ForeignKey("dbo.Burndowns", t => t.BurndownId, cascadeDelete: true)
                .Index(t => t.BurndownId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.HistoryLines", "BurndownId", "dbo.Burndowns");
            DropIndex("dbo.HistoryLines", new[] { "BurndownId" });
            DropTable("dbo.HistoryLines");
            DropTable("dbo.Burndowns");
        }
    }
}
