/* ============================================================================
 * AhuErp — создание схемы БД (SQL Server).
 *
 * Собрано по EF6 Code-First миграциям:
 *   1) 20260423121238_InitialCreate
 *   2) 20260423125626_AddEmployeeAuth
 *   3) 20260423131841_AddInventoryAndItTicket
 *   4) 20260423175847_AddVehicleTripDriverName
 *
 * Запуск в SQL Server Management Studio:
 *   1. Подключиться к DESKTOP-I1OTVEB\SQLEXPRESS (Windows Auth).
 *   2. File → Open → create-db.sql → F5.
 *   3. Проверить: SELECT name FROM sys.databases WHERE name = 'AhuErpDb';
 *
 * Скрипт идемпотентен: повторный запуск не упадёт на существующих объектах.
 * Идентификаторы (IDENTITY) EF6 создаёт как INT, автогенерация с 1.
 * ========================================================================== */

USE [master];
GO

IF DB_ID(N'AhuErpDb') IS NULL
BEGIN
    CREATE DATABASE [AhuErpDb];
END
GO

USE [AhuErpDb];
GO

/* ---------- 1. Employees ---------------------------------------------------- */
IF OBJECT_ID(N'dbo.Employees', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Employees
    (
        Id            INT            IDENTITY(1, 1) NOT NULL,
        FullName      NVARCHAR(256)  NOT NULL,
        [Position]    NVARCHAR(256)  NULL,
        [Role]        INT            NOT NULL CONSTRAINT DF_Employees_Role DEFAULT (0),
        PasswordHash  NVARCHAR(512)  NULL,
        CONSTRAINT PK_dbo_Employees PRIMARY KEY CLUSTERED (Id ASC)
    );
END
GO

/* ---------- 2. Documents (TPH: Document / ArchiveRequest / ItTicket) -------- */
IF OBJECT_ID(N'dbo.Documents', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Documents
    (
        Id                  INT             IDENTITY(1, 1) NOT NULL,
        [Type]              INT             NOT NULL,
        Title               NVARCHAR(512)   NOT NULL,
        CreationDate        DATETIME        NOT NULL,
        Deadline            DATETIME        NOT NULL,
        [Status]            INT             NOT NULL,
        AssignedEmployeeId  INT             NULL,
        HasPassportScan     BIT             NULL,
        HasWorkBookScan     BIT             NULL,
        ArchiveRequestKind  INT             NULL,
        AffectedEquipment   NVARCHAR(256)   NULL,
        ResolutionNotes     NVARCHAR(1024)  NULL,
        DocumentKind        NVARCHAR(128)   NOT NULL,
        CONSTRAINT PK_dbo_Documents PRIMARY KEY CLUSTERED (Id ASC)
    );

    ALTER TABLE dbo.Documents
        ADD CONSTRAINT [FK_dbo.Documents_dbo.Employees_AssignedEmployeeId]
        FOREIGN KEY (AssignedEmployeeId)
        REFERENCES dbo.Employees (Id); /* WillCascadeOnDelete(false) */

    CREATE NONCLUSTERED INDEX IX_Documents_AssignedEmployeeId
        ON dbo.Documents (AssignedEmployeeId);
END
GO

/* ---------- 3. Vehicles ---------------------------------------------------- */
IF OBJECT_ID(N'dbo.Vehicles', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Vehicles
    (
        Id             INT            IDENTITY(1, 1) NOT NULL,
        Model          NVARCHAR(128)  NOT NULL,
        LicensePlate   NVARCHAR(32)   NOT NULL,
        CurrentStatus  INT            NOT NULL,
        CONSTRAINT PK_dbo_Vehicles PRIMARY KEY CLUSTERED (Id ASC)
    );
END
GO

/* ---------- 4. VehicleTrips ------------------------------------------------ */
IF OBJECT_ID(N'dbo.VehicleTrips', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.VehicleTrips
    (
        Id          INT            IDENTITY(1, 1) NOT NULL,
        VehicleId   INT            NOT NULL,
        StartDate   DATETIME       NOT NULL,
        EndDate     DATETIME       NOT NULL,
        DocumentId  INT            NULL,
        DriverName  NVARCHAR(128)  NULL,
        CONSTRAINT PK_dbo_VehicleTrips PRIMARY KEY CLUSTERED (Id ASC)
    );

    ALTER TABLE dbo.VehicleTrips
        ADD CONSTRAINT [FK_dbo.VehicleTrips_dbo.Vehicles_VehicleId]
        FOREIGN KEY (VehicleId)
        REFERENCES dbo.Vehicles (Id)
        ON DELETE CASCADE; /* CascadeOnDelete(true) */

    ALTER TABLE dbo.VehicleTrips
        ADD CONSTRAINT [FK_dbo.VehicleTrips_dbo.Documents_DocumentId]
        FOREIGN KEY (DocumentId)
        REFERENCES dbo.Documents (Id); /* WillCascadeOnDelete(false) */

    CREATE NONCLUSTERED INDEX IX_VehicleTrips_VehicleId
        ON dbo.VehicleTrips (VehicleId);

    CREATE NONCLUSTERED INDEX IX_VehicleTrips_DocumentId
        ON dbo.VehicleTrips (DocumentId);
END
GO

/* ---------- 5. InventoryItems --------------------------------------------- */
IF OBJECT_ID(N'dbo.InventoryItems', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.InventoryItems
    (
        Id             INT            IDENTITY(1, 1) NOT NULL,
        [Name]         NVARCHAR(256)  NOT NULL,
        Category       INT            NOT NULL,
        TotalQuantity  INT            NOT NULL,
        CONSTRAINT PK_dbo_InventoryItems PRIMARY KEY CLUSTERED (Id ASC)
    );
END
GO

/* ---------- 6. InventoryTransactions -------------------------------------- */
IF OBJECT_ID(N'dbo.InventoryTransactions', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.InventoryTransactions
    (
        Id               INT       IDENTITY(1, 1) NOT NULL,
        InventoryItemId  INT       NOT NULL,
        DocumentId       INT       NULL,
        QuantityChanged  INT       NOT NULL,
        TransactionDate  DATETIME  NOT NULL,
        InitiatorId      INT       NOT NULL,
        CONSTRAINT PK_dbo_InventoryTransactions PRIMARY KEY CLUSTERED (Id ASC)
    );

    ALTER TABLE dbo.InventoryTransactions
        ADD CONSTRAINT [FK_dbo.InventoryTransactions_dbo.InventoryItems_InventoryItemId]
        FOREIGN KEY (InventoryItemId)
        REFERENCES dbo.InventoryItems (Id)
        ON DELETE CASCADE; /* CascadeOnDelete(true) */

    ALTER TABLE dbo.InventoryTransactions
        ADD CONSTRAINT [FK_dbo.InventoryTransactions_dbo.Documents_DocumentId]
        FOREIGN KEY (DocumentId)
        REFERENCES dbo.Documents (Id); /* WillCascadeOnDelete(false) */

    ALTER TABLE dbo.InventoryTransactions
        ADD CONSTRAINT [FK_dbo.InventoryTransactions_dbo.Employees_InitiatorId]
        FOREIGN KEY (InitiatorId)
        REFERENCES dbo.Employees (Id); /* WillCascadeOnDelete(false) */

    CREATE NONCLUSTERED INDEX IX_InventoryTransactions_InventoryItemId
        ON dbo.InventoryTransactions (InventoryItemId);

    CREATE NONCLUSTERED INDEX IX_InventoryTransactions_DocumentId
        ON dbo.InventoryTransactions (DocumentId);

    CREATE NONCLUSTERED INDEX IX_InventoryTransactions_InitiatorId
        ON dbo.InventoryTransactions (InitiatorId);
END
GO

/* ---------- 7. (необязательно) лёгкий сид-набор для проверки -------------- */
/* Расскоментируй блок ниже, если хочешь сразу получить несколько строк.
 * Пароль 'password' берётся из DemoDataSeeder; реальный хеш выставляет
 * приложение при первом запуске — здесь оставляем PasswordHash = NULL,
 * чтобы не конфликтовать с PBKDF2-алгоритмом.

IF NOT EXISTS (SELECT 1 FROM dbo.Employees)
BEGIN
    -- Role: Admin=0, Manager=1, Archivist=2, TechSupport=3, WarehouseManager=4
    INSERT INTO dbo.Employees (FullName, [Position], [Role], PasswordHash) VALUES
        (N'Администратор МКУ АХУ БМР', N'Администратор информационной системы', 0, NULL),
        (N'Стерликов Дмитрий Николаевич', N'Руководитель службы по информационно-техническому обеспечению', 1, NULL),
        (N'Бурдина Галина Николаевна', N'Начальник архивного отдела', 2, NULL),
        (N'Дорофеев Артем Валерьевич', N'Специалист-техник по компьютерным сетям и системам', 3, NULL),
        (N'Зайченко Татьяна Александровна', N'Специалист-техник по компьютерным сетям и системам', 4, NULL);
END
GO

IF NOT EXISTS (SELECT 1 FROM dbo.Vehicles)
BEGIN
    INSERT INTO dbo.Vehicles (Model, LicensePlate, CurrentStatus) VALUES
        (N'Lada Largus', N'А123БВ 64', 0),
        (N'ГАЗель NEXT', N'В777ТТ 64', 0),
        (N'УАЗ Патриот', N'Е111КХ 64', 1);
END
GO

IF NOT EXISTS (SELECT 1 FROM dbo.InventoryItems)
BEGIN
    INSERT INTO dbo.InventoryItems ([Name], Category, TotalQuantity) VALUES
        (N'Бумага A4 для документооборота', 0, 50),
        (N'Картридж для оргтехники',        1,  4),
        (N'Средство для уборки помещений',  2,  3);
END
GO
*/

PRINT N'AhuErpDb: схема создана / актуальна.';
GO
