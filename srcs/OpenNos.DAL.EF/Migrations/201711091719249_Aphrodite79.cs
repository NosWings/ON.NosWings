namespace OpenNos.DAL.EF.Migrations
{
    using System.Data.Entity.Migrations;

    public partial class Aphrodite79 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Character", "Elo", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Character", "Elo");
        }
    }
}
