namespace OpenNos.DAL.EF.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Aphrodite88 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.CharacterQuest", "IsMainQuest", c => c.Boolean(nullable: false));
            AlterColumn("dbo.Quest", "QuestType", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Quest", "QuestType", c => c.Byte(nullable: false));
            DropColumn("dbo.CharacterQuest", "IsMainQuest");
        }
    }
}
