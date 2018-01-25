namespace OpenNos.DAL.EF.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Aphrodite101 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Recipe", "ProduceItemVNum", c => c.Short(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Recipe", "ProduceItemVNum");
        }
    }
}
