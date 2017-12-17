namespace OpenNos.DAL.EF.Migrations
{
    using System.Data.Entity.Migrations;

    public partial class Aphrodite73 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Character", "IsPetAutoRelive", c => c.Boolean(nullable: false));
            AddColumn("dbo.Character", "IsPartnerAutoRelive", c => c.Boolean(nullable: false));
            
        }
        
        public override void Down()
        {
            DropColumn("dbo.Character", "IsPartnerAutoRelive");
            DropColumn("dbo.Character", "IsPetAutoRelive");
        }
    }
}
