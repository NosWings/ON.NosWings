using System.Data.Entity.Migrations;

namespace OpenNos.DAL.EF.Migrations
{
    public partial class Aphrodite34 : DbMigration
    {
        #region Methods

        public override void Down()
        {
            DropForeignKey("dbo.Nosmate", "CharacterId", "dbo.Character");
            DropForeignKey("dbo.Nosmate", "NpcMonsterVNum", "dbo.NpcMonster");
            DropIndex("dbo.Nosmate", new[] { "NpcMonsterVNum" });
            DropIndex("dbo.Nosmate", new[] { "CharacterId" });
            DropTable("dbo.Nosmate");
        }

        public override void Up()
        {
            CreateTable(
                    "dbo.Nosmate",
                    c => new
                    {
                        NosmateId = c.Long(false, true),
                        Attack = c.Byte(false),
                        CanPickUp = c.Boolean(false),
                        CharacterId = c.Long(false),
                        NpcMonsterVNum = c.Short(false),
                        Defence = c.Byte(false),
                        Experience = c.Long(false),
                        HasSkin = c.Boolean(false),
                        IsSummonable = c.Boolean(false),
                        Level = c.Byte(false),
                        Loyalty = c.Short(false),
                        MateType = c.Byte(false),
                        Name = c.String(maxLength: 255)
                    })
                .PrimaryKey(t => t.NosmateId)
                .ForeignKey("dbo.NpcMonster", t => t.NpcMonsterVNum)
                .ForeignKey("dbo.Character", t => t.CharacterId)
                .Index(t => t.CharacterId)
                .Index(t => t.NpcMonsterVNum);
        }

        #endregion
    }
}