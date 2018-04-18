using System.Data.Entity.Migrations;

namespace OpenNos.DAL.EF.Migrations
{
    public partial class Aphrodite18 : DbMigration
    {
        #region Methods

        public override void Down()
        {
            AddColumn("dbo.Account", "LastSession", c => c.Int(false));
        }

        public override void Up()
        {
            DropColumn("dbo.Account", "LastSession");
        }

        #endregion
    }
}