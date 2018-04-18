using System.Data.Entity.Migrations;

namespace OpenNos.DAL.EF.Migrations
{
    public partial class Aphrodite36 : DbMigration
    {
        #region Methods

        public override void Down()
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
                .PrimaryKey(t => t.NosmateId);

            DropForeignKey("dbo.Mate", "CharacterId", "dbo.Character");
            DropForeignKey("dbo.Mate", "NpcMonsterVNum", "dbo.NpcMonster");
            DropIndex("dbo.Mate", new[] { "NpcMonsterVNum" });
            DropIndex("dbo.Mate", new[] { "CharacterId" });
            DropTable("dbo.Mate");
            CreateIndex("dbo.Nosmate", "NpcMonsterVNum");
            CreateIndex("dbo.Nosmate", "CharacterId");
            AddForeignKey("dbo.Nosmate", "CharacterId", "dbo.Character", "CharacterId");
            AddForeignKey("dbo.Nosmate", "NpcMonsterVNum", "dbo.NpcMonster", "NpcMonsterVNum");
        }

        public override void Up()
        {
            DropForeignKey("dbo.Nosmate", "NpcMonsterVNum", "dbo.NpcMonster");
            DropForeignKey("dbo.Nosmate", "CharacterId", "dbo.Character");
            DropIndex("dbo.Nosmate", new[] { "CharacterId" });
            DropIndex("dbo.Nosmate", new[] { "NpcMonsterVNum" });
            CreateTable(
                    "dbo.Mate",
                    c => new
                    {
                        MateId = c.Long(false, true),
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
                .PrimaryKey(t => t.MateId)
                .ForeignKey("dbo.NpcMonster", t => t.NpcMonsterVNum)
                .ForeignKey("dbo.Character", t => t.CharacterId)
                .Index(t => t.CharacterId)
                .Index(t => t.NpcMonsterVNum);

            DropTable("dbo.Nosmate");
        }

        #endregion
    }
}