namespace OpenNos.DAL.EF.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Aphrodite103 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.QuestLog", "LastDaily", c => c.DateTime());
        }
        
        public override void Down()
        {
            DropColumn("dbo.QuestLog", "LastDaily");
        }
    }
}
