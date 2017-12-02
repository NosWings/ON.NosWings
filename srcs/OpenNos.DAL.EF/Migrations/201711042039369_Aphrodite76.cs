namespace OpenNos.DAL.EF.Migrations
{
    using System.Data.Entity.Migrations;

    public partial class Aphrodite76 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Quest", "InfoId", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Quest", "InfoId");
        }
    }
}
