namespace OpenNos.DAL.EF.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Aphrodite91 : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.CharacterQuest", "QuestId", "dbo.Quest");
            DropPrimaryKey("dbo.Quest");
            AlterColumn("dbo.Quest", "QuestId", c => c.Long(nullable: false, identity: true));
            AddPrimaryKey("dbo.Quest", "QuestId");
            AddForeignKey("dbo.CharacterQuest", "QuestId", "dbo.Quest", "QuestId", cascadeDelete: true);
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.CharacterQuest", "QuestId", "dbo.Quest");
            DropPrimaryKey("dbo.Quest");
            AlterColumn("dbo.Quest", "QuestId", c => c.Long(nullable: false));
            AddPrimaryKey("dbo.Quest", "QuestId");
            AddForeignKey("dbo.CharacterQuest", "QuestId", "dbo.Quest", "QuestId", cascadeDelete: true);
        }
    }
}
