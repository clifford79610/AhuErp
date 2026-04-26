namespace AhuErp.Core.Migrations
{
    using System.Data.Entity.Migrations;

    /// <summary>
    /// Phase 7 — переход к промышленному СЭД-уровню.
    /// Добавляет:
    /// <list type="bullet">
    ///   <item><description>Справочники: <c>Departments</c>, <c>DocumentTypeRefs</c>, <c>NomenclatureCases</c>.</description></item>
    ///   <item><description>Расширения <c>Documents</c>: регистрационные реквизиты, номенклатура, корреспондент, гриф доступа, маршрут согласования, документ-основание.</description></item>
    ///   <item><description>Связь документов с делами: <c>DocumentCaseLinks</c>.</description></item>
    ///   <item><description>Вложения с версиями: <c>DocumentAttachments</c>.</description></item>
    ///   <item><description>Резолюции и поручения: <c>DocumentResolutions</c>, <c>DocumentTasks</c>.</description></item>
    ///   <item><description>Маршруты согласования: <c>ApprovalRouteTemplates</c>, <c>ApprovalStages</c>, <c>DocumentApprovals</c>.</description></item>
    ///   <item><description>Журнал аудита с hash-цепочкой: <c>AuditLogs</c>.</description></item>
    ///   <item><description>Поле <c>BasisDocumentId</c> для связки хозяйственных операций с документом-основанием.</description></item>
    /// </list>
    /// Также переименовывает дискриминатор TPH-иерархии документов
    /// <c>DocumentKind</c> → <c>DocumentDiscriminator</c>, чтобы освободить
    /// «прикладное» имя <c>DocumentKind</c> для возможной будущей колонки и
    /// чётко отделить технический столбец EF от бизнес-полей.
    /// </summary>
    public partial class AddEnterpriseEDMSFeatures : DbMigration
    {
        public override void Up()
        {
            // ---------- 1. Departments ------------------------------------------------
            CreateTable(
                "dbo.Departments",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    Name = c.String(nullable: false, maxLength: 256),
                    ShortCode = c.String(maxLength: 16),
                    IsActive = c.Boolean(nullable: false),
                })
                .PrimaryKey(t => t.Id);

            // ---------- 2. DocumentTypeRefs -------------------------------------------
            CreateTable(
                "dbo.DocumentTypeRefs",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    Name = c.String(nullable: false, maxLength: 256),
                    ShortCode = c.String(maxLength: 16),
                    DefaultDirection = c.Int(nullable: false),
                    DefaultRetentionYears = c.Int(nullable: false),
                    RegistrationNumberTemplate = c.String(maxLength: 128),
                    IsActive = c.Boolean(nullable: false),
                })
                .PrimaryKey(t => t.Id);

            // ---------- 3. NomenclatureCases ------------------------------------------
            CreateTable(
                "dbo.NomenclatureCases",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    Index = c.String(nullable: false, maxLength: 32),
                    Title = c.String(nullable: false, maxLength: 512),
                    DepartmentId = c.Int(),
                    RetentionPeriodYears = c.Int(nullable: false),
                    Article = c.String(maxLength: 64),
                    Year = c.Int(nullable: false),
                    IsActive = c.Boolean(nullable: false),
                })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Departments", t => t.DepartmentId)
                .Index(t => t.DepartmentId);

            // ---------- 4. Documents — новые реквизиты --------------------------------
            // Дискриминатор переименован: DocumentKind -> DocumentDiscriminator.
            RenameColumn(table: "dbo.Documents", name: "DocumentKind", newName: "DocumentDiscriminator");
            AlterColumn("dbo.Documents", "DocumentDiscriminator",
                c => c.String(nullable: false, maxLength: 128));

            AddColumn("dbo.Documents", "Direction", c => c.Int(nullable: false, defaultValue: 0));
            AddColumn("dbo.Documents", "AccessLevel", c => c.Int(nullable: false, defaultValue: 0));
            AddColumn("dbo.Documents", "RegistrationNumber", c => c.String(maxLength: 64));
            AddColumn("dbo.Documents", "RegistrationDate", c => c.DateTime());
            AddColumn("dbo.Documents", "DocumentTypeRefId", c => c.Int());
            AddColumn("dbo.Documents", "NomenclatureCaseId", c => c.Int());
            AddColumn("dbo.Documents", "AuthorId", c => c.Int());
            AddColumn("dbo.Documents", "Summary", c => c.String(maxLength: 4000));
            AddColumn("dbo.Documents", "Correspondent", c => c.String(maxLength: 512));
            AddColumn("dbo.Documents", "IncomingNumber", c => c.String(maxLength: 64));
            AddColumn("dbo.Documents", "IncomingDate", c => c.DateTime());
            AddColumn("dbo.Documents", "BasisDocumentId", c => c.Int());
            AddColumn("dbo.Documents", "ApprovalStatus", c => c.Int(nullable: false, defaultValue: 0));

            CreateIndex("dbo.Documents", "DocumentTypeRefId");
            CreateIndex("dbo.Documents", "NomenclatureCaseId");
            CreateIndex("dbo.Documents", "AuthorId");
            CreateIndex("dbo.Documents", "BasisDocumentId");

            AddForeignKey("dbo.Documents", "DocumentTypeRefId", "dbo.DocumentTypeRefs", "Id");
            AddForeignKey("dbo.Documents", "NomenclatureCaseId", "dbo.NomenclatureCases", "Id");
            AddForeignKey("dbo.Documents", "AuthorId", "dbo.Employees", "Id");
            AddForeignKey("dbo.Documents", "BasisDocumentId", "dbo.Documents", "Id");

            // ---------- 5. DocumentCaseLinks ------------------------------------------
            CreateTable(
                "dbo.DocumentCaseLinks",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    DocumentId = c.Int(nullable: false),
                    NomenclatureCaseId = c.Int(nullable: false),
                    LinkedAt = c.DateTime(nullable: false),
                    LinkedById = c.Int(),
                    IsPrimary = c.Boolean(nullable: false),
                })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Documents", t => t.DocumentId, cascadeDelete: true)
                .ForeignKey("dbo.NomenclatureCases", t => t.NomenclatureCaseId)
                .ForeignKey("dbo.Employees", t => t.LinkedById)
                .Index(t => t.DocumentId)
                .Index(t => t.NomenclatureCaseId)
                .Index(t => t.LinkedById);

            // ---------- 6. DocumentAttachments ----------------------------------------
            CreateTable(
                "dbo.DocumentAttachments",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    DocumentId = c.Int(nullable: false),
                    AttachmentGroupId = c.Int(nullable: false),
                    FileName = c.String(nullable: false, maxLength: 512),
                    StoragePath = c.String(nullable: false, maxLength: 1024),
                    VersionNumber = c.Int(nullable: false),
                    IsCurrentVersion = c.Boolean(nullable: false),
                    UploadedAt = c.DateTime(nullable: false),
                    UploadedById = c.Int(nullable: false),
                    Comment = c.String(maxLength: 1024),
                    Hash = c.String(maxLength: 128),
                    FileType = c.Int(nullable: false),
                    SizeBytes = c.Long(nullable: false),
                })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Documents", t => t.DocumentId, cascadeDelete: true)
                .ForeignKey("dbo.Employees", t => t.UploadedById)
                .Index(t => t.DocumentId)
                .Index(t => t.UploadedById)
                .Index(t => t.AttachmentGroupId);

            // ---------- 7. DocumentResolutions ----------------------------------------
            CreateTable(
                "dbo.DocumentResolutions",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    DocumentId = c.Int(nullable: false),
                    AuthorId = c.Int(nullable: false),
                    Text = c.String(nullable: false, maxLength: 2048),
                    IssuedAt = c.DateTime(nullable: false),
                })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Documents", t => t.DocumentId, cascadeDelete: true)
                .ForeignKey("dbo.Employees", t => t.AuthorId)
                .Index(t => t.DocumentId)
                .Index(t => t.AuthorId);

            // ---------- 8. DocumentTasks ----------------------------------------------
            CreateTable(
                "dbo.DocumentTasks",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    DocumentId = c.Int(nullable: false),
                    ResolutionId = c.Int(),
                    ParentTaskId = c.Int(),
                    AuthorId = c.Int(nullable: false),
                    ExecutorId = c.Int(nullable: false),
                    ControllerId = c.Int(),
                    CoExecutors = c.String(maxLength: 1024),
                    Description = c.String(nullable: false, maxLength: 2048),
                    CreatedAt = c.DateTime(nullable: false),
                    Deadline = c.DateTime(nullable: false),
                    Status = c.Int(nullable: false),
                    CompletedAt = c.DateTime(),
                    ReportText = c.String(maxLength: 2048),
                    IsCritical = c.Boolean(nullable: false),
                })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Documents", t => t.DocumentId)
                .ForeignKey("dbo.DocumentResolutions", t => t.ResolutionId)
                .ForeignKey("dbo.DocumentTasks", t => t.ParentTaskId)
                .ForeignKey("dbo.Employees", t => t.AuthorId)
                .ForeignKey("dbo.Employees", t => t.ExecutorId)
                .ForeignKey("dbo.Employees", t => t.ControllerId)
                .Index(t => t.DocumentId)
                .Index(t => t.ResolutionId)
                .Index(t => t.ParentTaskId)
                .Index(t => t.AuthorId)
                .Index(t => t.ExecutorId)
                .Index(t => t.ControllerId);

            // ---------- 9. ApprovalRouteTemplates -------------------------------------
            CreateTable(
                "dbo.ApprovalRouteTemplates",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    Name = c.String(nullable: false, maxLength: 256),
                    Description = c.String(maxLength: 1024),
                    IsActive = c.Boolean(nullable: false),
                    DocumentTypeRefId = c.Int(),
                })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.DocumentTypeRefs", t => t.DocumentTypeRefId)
                .Index(t => t.DocumentTypeRefId);

            // ---------- 10. ApprovalStages --------------------------------------------
            CreateTable(
                "dbo.ApprovalStages",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    RouteTemplateId = c.Int(nullable: false),
                    Order = c.Int(nullable: false),
                    IsParallel = c.Boolean(nullable: false),
                    ApproverEmployeeId = c.Int(),
                    ApproverRole = c.Int(),
                    Description = c.String(maxLength: 512),
                })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ApprovalRouteTemplates", t => t.RouteTemplateId, cascadeDelete: true)
                .ForeignKey("dbo.Employees", t => t.ApproverEmployeeId)
                .Index(t => t.RouteTemplateId)
                .Index(t => t.ApproverEmployeeId);

            // ---------- 11. DocumentApprovals -----------------------------------------
            CreateTable(
                "dbo.DocumentApprovals",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    DocumentId = c.Int(nullable: false),
                    StageId = c.Int(),
                    Order = c.Int(nullable: false),
                    IsParallel = c.Boolean(nullable: false),
                    ApproverId = c.Int(nullable: false),
                    Decision = c.Int(nullable: false),
                    Comment = c.String(maxLength: 2048),
                    DecisionDate = c.DateTime(),
                })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Documents", t => t.DocumentId, cascadeDelete: true)
                .ForeignKey("dbo.ApprovalStages", t => t.StageId)
                .ForeignKey("dbo.Employees", t => t.ApproverId)
                .Index(t => t.DocumentId)
                .Index(t => t.StageId)
                .Index(t => t.ApproverId);

            // ---------- 12. AuditLogs -------------------------------------------------
            CreateTable(
                "dbo.AuditLogs",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    Timestamp = c.DateTime(nullable: false),
                    UserId = c.Int(),
                    ActionType = c.Int(nullable: false),
                    EntityType = c.String(maxLength: 128),
                    EntityId = c.Int(),
                    OldValues = c.String(maxLength: 4000),
                    NewValues = c.String(maxLength: 4000),
                    Details = c.String(maxLength: 1024),
                    Hash = c.String(maxLength: 128),
                    PreviousHash = c.String(maxLength: 128),
                })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Employees", t => t.UserId)
                .Index(t => t.UserId)
                .Index(t => t.Timestamp)
                .Index(t => t.EntityType);

            // ---------- 13. BasisDocumentId на хозяйственных операциях ----------------
            // (Документ-основание для путевых листов / складских транзакций /
            //  IT-тикетов / архивных запросов уже частично был — добавляем
            //  единое имя поля BasisDocumentId на InventoryTransactions и
            //  VehicleTrips. В TPH-иерархии (ArchiveRequest, ItTicket) это
            //  поле уже доступно через Document.BasisDocumentId.)
            AddColumn("dbo.InventoryTransactions", "BasisDocumentId", c => c.Int());
            CreateIndex("dbo.InventoryTransactions", "BasisDocumentId");
            AddForeignKey("dbo.InventoryTransactions", "BasisDocumentId", "dbo.Documents", "Id");

            AddColumn("dbo.VehicleTrips", "BasisDocumentId", c => c.Int());
            CreateIndex("dbo.VehicleTrips", "BasisDocumentId");
            AddForeignKey("dbo.VehicleTrips", "BasisDocumentId", "dbo.Documents", "Id");
        }

        public override void Down()
        {
            DropForeignKey("dbo.VehicleTrips", "BasisDocumentId", "dbo.Documents");
            DropIndex("dbo.VehicleTrips", new[] { "BasisDocumentId" });
            DropColumn("dbo.VehicleTrips", "BasisDocumentId");

            DropForeignKey("dbo.InventoryTransactions", "BasisDocumentId", "dbo.Documents");
            DropIndex("dbo.InventoryTransactions", new[] { "BasisDocumentId" });
            DropColumn("dbo.InventoryTransactions", "BasisDocumentId");

            DropForeignKey("dbo.AuditLogs", "UserId", "dbo.Employees");
            DropIndex("dbo.AuditLogs", new[] { "EntityType" });
            DropIndex("dbo.AuditLogs", new[] { "Timestamp" });
            DropIndex("dbo.AuditLogs", new[] { "UserId" });
            DropTable("dbo.AuditLogs");

            DropForeignKey("dbo.DocumentApprovals", "ApproverId", "dbo.Employees");
            DropForeignKey("dbo.DocumentApprovals", "StageId", "dbo.ApprovalStages");
            DropForeignKey("dbo.DocumentApprovals", "DocumentId", "dbo.Documents");
            DropIndex("dbo.DocumentApprovals", new[] { "ApproverId" });
            DropIndex("dbo.DocumentApprovals", new[] { "StageId" });
            DropIndex("dbo.DocumentApprovals", new[] { "DocumentId" });
            DropTable("dbo.DocumentApprovals");

            DropForeignKey("dbo.ApprovalStages", "ApproverEmployeeId", "dbo.Employees");
            DropForeignKey("dbo.ApprovalStages", "RouteTemplateId", "dbo.ApprovalRouteTemplates");
            DropIndex("dbo.ApprovalStages", new[] { "ApproverEmployeeId" });
            DropIndex("dbo.ApprovalStages", new[] { "RouteTemplateId" });
            DropTable("dbo.ApprovalStages");

            DropForeignKey("dbo.ApprovalRouteTemplates", "DocumentTypeRefId", "dbo.DocumentTypeRefs");
            DropIndex("dbo.ApprovalRouteTemplates", new[] { "DocumentTypeRefId" });
            DropTable("dbo.ApprovalRouteTemplates");

            DropForeignKey("dbo.DocumentTasks", "ControllerId", "dbo.Employees");
            DropForeignKey("dbo.DocumentTasks", "ExecutorId", "dbo.Employees");
            DropForeignKey("dbo.DocumentTasks", "AuthorId", "dbo.Employees");
            DropForeignKey("dbo.DocumentTasks", "ParentTaskId", "dbo.DocumentTasks");
            DropForeignKey("dbo.DocumentTasks", "ResolutionId", "dbo.DocumentResolutions");
            DropForeignKey("dbo.DocumentTasks", "DocumentId", "dbo.Documents");
            DropIndex("dbo.DocumentTasks", new[] { "ControllerId" });
            DropIndex("dbo.DocumentTasks", new[] { "ExecutorId" });
            DropIndex("dbo.DocumentTasks", new[] { "AuthorId" });
            DropIndex("dbo.DocumentTasks", new[] { "ParentTaskId" });
            DropIndex("dbo.DocumentTasks", new[] { "ResolutionId" });
            DropIndex("dbo.DocumentTasks", new[] { "DocumentId" });
            DropTable("dbo.DocumentTasks");

            DropForeignKey("dbo.DocumentResolutions", "AuthorId", "dbo.Employees");
            DropForeignKey("dbo.DocumentResolutions", "DocumentId", "dbo.Documents");
            DropIndex("dbo.DocumentResolutions", new[] { "AuthorId" });
            DropIndex("dbo.DocumentResolutions", new[] { "DocumentId" });
            DropTable("dbo.DocumentResolutions");

            DropForeignKey("dbo.DocumentAttachments", "UploadedById", "dbo.Employees");
            DropForeignKey("dbo.DocumentAttachments", "DocumentId", "dbo.Documents");
            DropIndex("dbo.DocumentAttachments", new[] { "AttachmentGroupId" });
            DropIndex("dbo.DocumentAttachments", new[] { "UploadedById" });
            DropIndex("dbo.DocumentAttachments", new[] { "DocumentId" });
            DropTable("dbo.DocumentAttachments");

            DropForeignKey("dbo.DocumentCaseLinks", "LinkedById", "dbo.Employees");
            DropForeignKey("dbo.DocumentCaseLinks", "NomenclatureCaseId", "dbo.NomenclatureCases");
            DropForeignKey("dbo.DocumentCaseLinks", "DocumentId", "dbo.Documents");
            DropIndex("dbo.DocumentCaseLinks", new[] { "LinkedById" });
            DropIndex("dbo.DocumentCaseLinks", new[] { "NomenclatureCaseId" });
            DropIndex("dbo.DocumentCaseLinks", new[] { "DocumentId" });
            DropTable("dbo.DocumentCaseLinks");

            DropForeignKey("dbo.Documents", "BasisDocumentId", "dbo.Documents");
            DropForeignKey("dbo.Documents", "AuthorId", "dbo.Employees");
            DropForeignKey("dbo.Documents", "NomenclatureCaseId", "dbo.NomenclatureCases");
            DropForeignKey("dbo.Documents", "DocumentTypeRefId", "dbo.DocumentTypeRefs");
            DropIndex("dbo.Documents", new[] { "BasisDocumentId" });
            DropIndex("dbo.Documents", new[] { "AuthorId" });
            DropIndex("dbo.Documents", new[] { "NomenclatureCaseId" });
            DropIndex("dbo.Documents", new[] { "DocumentTypeRefId" });
            DropColumn("dbo.Documents", "ApprovalStatus");
            DropColumn("dbo.Documents", "BasisDocumentId");
            DropColumn("dbo.Documents", "IncomingDate");
            DropColumn("dbo.Documents", "IncomingNumber");
            DropColumn("dbo.Documents", "Correspondent");
            DropColumn("dbo.Documents", "Summary");
            DropColumn("dbo.Documents", "AuthorId");
            DropColumn("dbo.Documents", "NomenclatureCaseId");
            DropColumn("dbo.Documents", "DocumentTypeRefId");
            DropColumn("dbo.Documents", "RegistrationDate");
            DropColumn("dbo.Documents", "RegistrationNumber");
            DropColumn("dbo.Documents", "AccessLevel");
            DropColumn("dbo.Documents", "Direction");
            RenameColumn(table: "dbo.Documents", name: "DocumentDiscriminator", newName: "DocumentKind");

            DropForeignKey("dbo.NomenclatureCases", "DepartmentId", "dbo.Departments");
            DropIndex("dbo.NomenclatureCases", new[] { "DepartmentId" });
            DropTable("dbo.NomenclatureCases");

            DropTable("dbo.DocumentTypeRefs");

            DropTable("dbo.Departments");
        }
    }
}
