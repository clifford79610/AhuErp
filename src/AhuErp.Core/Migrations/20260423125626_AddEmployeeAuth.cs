namespace AhuErp.Core.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddEmployeeAuth : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Employees", "Role", c => c.Int(nullable: false));
            AddColumn("dbo.Employees", "PasswordHash", c => c.String(maxLength: 512));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Employees", "PasswordHash");
            DropColumn("dbo.Employees", "Role");
        }
    }
}
