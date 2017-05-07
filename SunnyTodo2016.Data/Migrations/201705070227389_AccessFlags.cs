namespace SunnyTodo2016.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AccessFlags : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Burndowns", "IsPublicViewable", c => c.Boolean(nullable: false));
            AddColumn("dbo.Burndowns", "IsPublicEditable", c => c.Boolean(nullable: false));
            Sql("update dbo.Burndowns set IsPublicViewable=0,IsPublicEditable=0 where OwnerUserId<>'00000000-0000-0000-0000-000000000000'");
            Sql("update dbo.Burndowns set IsPublicViewable=1,IsPublicEditable=1 where OwnerUserId='00000000-0000-0000-0000-000000000000'");
        }

        public override void Down()
        {
            DropColumn("dbo.Burndowns", "IsPublicEditable");
            DropColumn("dbo.Burndowns", "IsPublicViewable");
        }
    }
}
