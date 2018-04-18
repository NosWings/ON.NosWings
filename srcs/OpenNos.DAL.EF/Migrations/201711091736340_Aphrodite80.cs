using System.Data.Entity.Migrations;

namespace OpenNos.DAL.EF.Migrations
{
    public partial class Aphrodite80 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Account", "BankMoney", c => c.Long(false));
            AddColumn("dbo.Account", "Money", c => c.Long(false));
        }

        public override void Down()
        {
            DropColumn("dbo.Account", "Money");
            DropColumn("dbo.Account", "BankMoney");
        }
    }
}