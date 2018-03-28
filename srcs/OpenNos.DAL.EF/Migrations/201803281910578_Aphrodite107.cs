namespace OpenNos.DAL.EF.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Aphrodite107 : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.BazaarItem", "Amount", c => c.Short(nullable: false));
            AlterColumn("dbo.Mail", "AttachmentAmount", c => c.Short(nullable: false));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Mail", "AttachmentAmount", c => c.Byte(nullable: false));
            AlterColumn("dbo.BazaarItem", "Amount", c => c.Byte(nullable: false));
        }
    }
}
