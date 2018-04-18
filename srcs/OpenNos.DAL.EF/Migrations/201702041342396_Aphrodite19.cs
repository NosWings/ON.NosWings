using System.Data.Entity.Migrations;

namespace OpenNos.DAL.EF.Migrations
{
    public partial class Aphrodite19 : DbMigration
    {
        #region Methods

        public override void Down()
        {
            DropForeignKey("dbo.SkillCard", "SkillVNum", "dbo.Skill");
            DropForeignKey("dbo.SkillCard", "CardId", "dbo.Card");
            DropIndex("dbo.SkillCard", new[] { "CardId" });
            DropIndex("dbo.SkillCard", new[] { "SkillVNum" });
            DropTable("dbo.Card");
            DropTable("dbo.SkillCard");
        }

        public override void Up()
        {
            CreateTable(
                    "dbo.SkillCard",
                    c => new
                    {
                        SkillVNum = c.Short(false),
                        CardId = c.Short(false),
                        CardChance = c.Short(false)
                    })
                .PrimaryKey(t => new { t.SkillVNum, t.CardId })
                .ForeignKey("dbo.Card", t => t.CardId)
                .ForeignKey("dbo.Skill", t => t.SkillVNum)
                .Index(t => t.SkillVNum)
                .Index(t => t.CardId);

            CreateTable(
                    "dbo.Card",
                    c => new
                    {
                        CardId = c.Short(false),
                        Duration = c.Int(false),
                        EffectId = c.Int(false),
                        FirstData = c.Short(false),
                        Level = c.Byte(false),
                        Name = c.String(maxLength: 255),
                        Period = c.Short(false),
                        Propability = c.Byte(false),
                        SecondData = c.Short(false),
                        SubType = c.Byte(false),
                        Type = c.Byte(false)
                    })
                .PrimaryKey(t => t.CardId);
        }

        #endregion
    }
}