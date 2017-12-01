namespace OpenNos.DAL.EF.Migrations
{
    using System.Data.Entity.Migrations;

    public partial class Aphrodite82 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Character", "Prefix", c => c.String(maxLength: 25));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Character", "Prefix");
        }
    }
}
