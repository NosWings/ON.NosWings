namespace OpenNos.DAL.EF.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class Aphrodite72 : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.RollGeneratedItem", "MinimumOriginalItemRare", c => c.Short(nullable: false));
            AlterColumn("dbo.RollGeneratedItem", "MaximumOriginalItemRare", c => c.Short(nullable: false));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.RollGeneratedItem", "MaximumOriginalItemRare", c => c.Byte(nullable: false));
            AlterColumn("dbo.RollGeneratedItem", "MinimumOriginalItemRare", c => c.Byte(nullable: false));
        }
    }
}
