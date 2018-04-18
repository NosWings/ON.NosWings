using System.Data.Entity.Migrations;

namespace OpenNos.DAL.EF.Migrations
{
    public partial class Aphrodite41 : DbMigration
    {
        #region Methods

        public override void Down()
        {
            DropColumn("dbo.NpcMonster", "HeroXP");
        }

        public override void Up()
        {
            AddColumn("dbo.NpcMonster", "HeroXP", c => c.Int(false));
        }

        #endregion
    }
}