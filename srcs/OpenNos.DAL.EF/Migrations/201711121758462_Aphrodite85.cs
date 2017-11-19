namespace OpenNos.DAL.EF.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Aphrodite85 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Quest", "StartDialogId", c => c.Int());
            AddColumn("dbo.Quest", "LevelMin", c => c.Byte(nullable: false));
            AddColumn("dbo.Quest", "LevelMax", c => c.Byte(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Quest", "LevelMax");
            DropColumn("dbo.Quest", "LevelMin");
            DropColumn("dbo.Quest", "StartDialogId");
        }
    }
}
