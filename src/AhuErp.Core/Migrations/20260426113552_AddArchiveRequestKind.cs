namespace AhuErp.Core.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddArchiveRequestKind : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Documents", "ArchiveRequestKind", c => c.Int());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Documents", "ArchiveRequestKind");
        }
    }
}
