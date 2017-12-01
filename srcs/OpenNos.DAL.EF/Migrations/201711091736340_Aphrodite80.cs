namespace OpenNos.DAL.EF.Migrations
{
    using System.Data.Entity.Migrations;

    public partial class Aphrodite80 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Account", "BankMoney", c => c.Long(nullable: false));
            AddColumn("dbo.Account", "Money", c => c.Long(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Account", "Money");
            DropColumn("dbo.Account", "BankMoney");
        }
    }
}
