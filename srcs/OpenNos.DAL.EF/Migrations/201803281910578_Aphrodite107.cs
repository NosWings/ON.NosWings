using System.Data.Entity.Migrations;

namespace OpenNos.DAL.EF.Migrations
{
    public partial class Aphrodite107 : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.BazaarItem", "Amount", c => c.Short(false));
            AlterColumn("dbo.Mail", "AttachmentAmount", c => c.Short(false));
        }

        public override void Down()
        {
            AlterColumn("dbo.Mail", "AttachmentAmount", c => c.Byte(false));
            AlterColumn("dbo.BazaarItem", "Amount", c => c.Byte(false));
        }
    }
}