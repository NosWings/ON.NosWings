using System.Data.Entity.Migrations;

namespace OpenNos.DAL.EF.Migrations
{
    public partial class Aphrodite29 : DbMigration
    {
        #region Methods

        public override void Down()
        {
            DropForeignKey("dbo.GeneralLog", "AccountId", "dbo.Account");
            DropIndex("dbo.GeneralLog", new[] { "AccountId" });
            AlterColumn("dbo.GeneralLog", "AccountId", c => c.Long(false));
            CreateIndex("dbo.GeneralLog", "AccountId");
            AddForeignKey("dbo.GeneralLog", "AccountId", "dbo.Account", "AccountId", true);
        }

        public override void Up()
        {
            DropForeignKey("dbo.GeneralLog", "AccountId", "dbo.Account");
            DropIndex("dbo.GeneralLog", new[] { "AccountId" });
            AlterColumn("dbo.GeneralLog", "AccountId", c => c.Long());
            CreateIndex("dbo.GeneralLog", "AccountId");
            AddForeignKey("dbo.GeneralLog", "AccountId", "dbo.Account", "AccountId");
        }

        #endregion
    }
}