using System.Data.Entity.Migrations;

namespace OpenNos.DAL.EF.Migrations
{
    public partial class Aphrodite32 : DbMigration
    {
        #region Methods

        public override void Down()
        {
            AlterColumn("dbo.Card", "Type", c => c.Byte(false));
            AlterColumn("dbo.Card", "SecondData", c => c.Short(false));
            AlterColumn("dbo.Card", "FirstData", c => c.Short(false));
        }

        public override void Up()
        {
            AlterColumn("dbo.Card", "FirstData", c => c.Int(false));
            AlterColumn("dbo.Card", "SecondData", c => c.Int(false));
            AlterColumn("dbo.Card", "Type", c => c.Short(false));
        }

        #endregion
    }
}