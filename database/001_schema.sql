-- ============================================================
-- AccountingERP - SQL Server 2022 Schema
-- File: 001_schema.sql
-- Description: Full DDL for multi-tenant accounting ERP
-- Encoding: UTF-8 (nvarchar used throughout for Serbian chars)
-- ============================================================

USE master;
GO

IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'AccountingERP')
BEGIN
    CREATE DATABASE AccountingERP
        COLLATE Serbian_Cyrillic_100_CI_AS;
END
GO

USE AccountingERP;
GO

-- ============================================================
-- SCHEMA
-- ============================================================

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = N'erp')
    EXEC('CREATE SCHEMA erp AUTHORIZATION dbo;');
GO

-- ============================================================
-- TABLE: erp.Tenants
-- ============================================================

IF OBJECT_ID(N'erp.Tenants', N'U') IS NULL
BEGIN
    CREATE TABLE erp.Tenants (
        Id               INT              NOT NULL IDENTITY(1,1),
        Name             NVARCHAR(200)    NOT NULL,
        PIB              CHAR(9)          NOT NULL,
        MaticniBroj      CHAR(8)          NULL,
        Address          NVARCHAR(300)    NULL,
        City             NVARCHAR(100)    NULL,
        Country          NVARCHAR(100)    NOT NULL CONSTRAINT DF_Tenants_Country DEFAULT (N'Serbia'),
        Email            NVARCHAR(254)    NULL,
        Phone            NVARCHAR(50)     NULL,
        IsActive         BIT              NOT NULL CONSTRAINT DF_Tenants_IsActive DEFAULT (1),
        SubscriptionPlan NVARCHAR(50)     NOT NULL CONSTRAINT DF_Tenants_Plan DEFAULT (N'Trial'),
        CreatedAt        DATETIME2(7)     NOT NULL CONSTRAINT DF_Tenants_CreatedAt DEFAULT (SYSUTCDATETIME()),
        UpdatedAt        DATETIME2(7)     NOT NULL CONSTRAINT DF_Tenants_UpdatedAt DEFAULT (SYSUTCDATETIME()),

        CONSTRAINT PK_Tenants PRIMARY KEY CLUSTERED (Id),
        CONSTRAINT UQ_Tenants_PIB UNIQUE (PIB),
        CONSTRAINT CK_Tenants_PIB CHECK (PIB NOT LIKE '%[^0-9]%' AND LEN(PIB) = 9),
        CONSTRAINT CK_Tenants_MaticniBroj CHECK (MaticniBroj IS NULL OR (MaticniBroj NOT LIKE '%[^0-9]%' AND LEN(MaticniBroj) = 8))
    );
END
GO

-- ============================================================
-- TABLE: erp.Users
-- ============================================================

IF OBJECT_ID(N'erp.Users', N'U') IS NULL
BEGIN
    CREATE TABLE erp.Users (
        Id                   INT              NOT NULL IDENTITY(1,1),
        TenantId             INT              NOT NULL,
        Username             NVARCHAR(100)    NOT NULL,
        Email                NVARCHAR(254)    NOT NULL,
        PasswordHash         NVARCHAR(512)    NOT NULL,  -- BCrypt hash
        Role                 NVARCHAR(20)     NOT NULL CONSTRAINT DF_Users_Role DEFAULT (N'Viewer'),
        FirstName            NVARCHAR(100)    NULL,
        LastName             NVARCHAR(100)    NULL,
        IsActive             BIT              NOT NULL CONSTRAINT DF_Users_IsActive DEFAULT (1),
        EmailConfirmed       BIT              NOT NULL CONSTRAINT DF_Users_EmailConfirmed DEFAULT (0),
        TwoFactorEnabled     BIT              NOT NULL CONSTRAINT DF_Users_2FA DEFAULT (0),
        RefreshToken         NVARCHAR(512)    NULL,
        RefreshTokenExpiry   DATETIME2(7)     NULL,
        LastLoginAt          DATETIME2(7)     NULL,
        FailedLoginAttempts  TINYINT          NOT NULL CONSTRAINT DF_Users_FailedLogins DEFAULT (0),
        LockedUntil          DATETIME2(7)     NULL,
        CreatedAt            DATETIME2(7)     NOT NULL CONSTRAINT DF_Users_CreatedAt DEFAULT (SYSUTCDATETIME()),
        UpdatedAt            DATETIME2(7)     NOT NULL CONSTRAINT DF_Users_UpdatedAt DEFAULT (SYSUTCDATETIME()),

        CONSTRAINT PK_Users PRIMARY KEY CLUSTERED (Id),
        CONSTRAINT FK_Users_Tenants FOREIGN KEY (TenantId)
            REFERENCES erp.Tenants (Id) ON DELETE CASCADE ON UPDATE NO ACTION,
        CONSTRAINT CK_Users_Role CHECK (Role IN (N'Admin', N'Accountant', N'Viewer')),
        CONSTRAINT CK_Users_FailedLogins CHECK (FailedLoginAttempts >= 0)
    );

    CREATE UNIQUE INDEX UQ_Users_TenantUsername
        ON erp.Users (TenantId, Username);

    CREATE UNIQUE INDEX UQ_Users_TenantEmail
        ON erp.Users (TenantId, Email);

    CREATE INDEX IX_Users_TenantId
        ON erp.Users (TenantId);
END
GO

-- ============================================================
-- TABLE: erp.Clients
-- ============================================================

IF OBJECT_ID(N'erp.Clients', N'U') IS NULL
BEGIN
    CREATE TABLE erp.Clients (
        Id           INT              NOT NULL IDENTITY(1,1),
        TenantId     INT              NOT NULL,
        Name         NVARCHAR(300)    NOT NULL,
        PIB          CHAR(9)          NULL,
        MaticniBroj  CHAR(8)          NULL,
        Address      NVARCHAR(300)    NULL,
        City         NVARCHAR(100)    NULL,
        Country      NVARCHAR(100)    NOT NULL CONSTRAINT DF_Clients_Country DEFAULT (N'Serbia'),
        IBAN         NVARCHAR(34)     NULL,
        Email        NVARCHAR(254)    NULL,
        Phone        NVARCHAR(50)     NULL,
        ContactPerson NVARCHAR(200)   NULL,
        ClientType   NVARCHAR(20)     NOT NULL CONSTRAINT DF_Clients_Type DEFAULT (N'Customer'),
        IsActive     BIT              NOT NULL CONSTRAINT DF_Clients_IsActive DEFAULT (1),
        Notes        NVARCHAR(MAX)    NULL,
        CreatedAt    DATETIME2(7)     NOT NULL CONSTRAINT DF_Clients_CreatedAt DEFAULT (SYSUTCDATETIME()),
        UpdatedAt    DATETIME2(7)     NOT NULL CONSTRAINT DF_Clients_UpdatedAt DEFAULT (SYSUTCDATETIME()),

        CONSTRAINT PK_Clients PRIMARY KEY CLUSTERED (Id),
        CONSTRAINT FK_Clients_Tenants FOREIGN KEY (TenantId)
            REFERENCES erp.Tenants (Id) ON DELETE CASCADE ON UPDATE NO ACTION,
        CONSTRAINT CK_Clients_Type CHECK (ClientType IN (N'Customer', N'Supplier', N'Both')),
        CONSTRAINT CK_Clients_PIB CHECK (PIB IS NULL OR (PIB NOT LIKE '%[^0-9]%' AND LEN(PIB) = 9)),
        CONSTRAINT CK_Clients_MaticniBroj CHECK (MaticniBroj IS NULL OR (MaticniBroj NOT LIKE '%[^0-9]%' AND LEN(MaticniBroj) = 8)),
        CONSTRAINT CK_Clients_IBAN CHECK (IBAN IS NULL OR LEN(IBAN) BETWEEN 15 AND 34)
    );

    CREATE INDEX IX_Clients_TenantId
        ON erp.Clients (TenantId);

    CREATE INDEX IX_Clients_TenantId_PIB
        ON erp.Clients (TenantId, PIB)
        WHERE PIB IS NOT NULL;

    CREATE INDEX IX_Clients_TenantId_IsActive
        ON erp.Clients (TenantId, IsActive);
END
GO

-- ============================================================
-- TABLE: erp.ChartOfAccounts
-- ============================================================

IF OBJECT_ID(N'erp.ChartOfAccounts', N'U') IS NULL
BEGIN
    CREATE TABLE erp.ChartOfAccounts (
        Id          INT              NOT NULL IDENTITY(1,1),
        -- TenantId = 0 means system/template account shared by all tenants
        TenantId    INT              NOT NULL CONSTRAINT DF_COA_TenantId DEFAULT (0),
        Code        VARCHAR(10)      NOT NULL,   -- e.g. '241', '4300'
        Name        NVARCHAR(200)    NOT NULL,
        Type        NVARCHAR(20)     NOT NULL,
        ParentCode  VARCHAR(10)      NULL,
        Level       TINYINT          NOT NULL CONSTRAINT DF_COA_Level DEFAULT (1),
        IsActive    BIT              NOT NULL CONSTRAINT DF_COA_IsActive DEFAULT (1),
        CreatedAt   DATETIME2(7)     NOT NULL CONSTRAINT DF_COA_CreatedAt DEFAULT (SYSUTCDATETIME()),
        UpdatedAt   DATETIME2(7)     NOT NULL CONSTRAINT DF_COA_UpdatedAt DEFAULT (SYSUTCDATETIME()),

        CONSTRAINT PK_ChartOfAccounts PRIMARY KEY CLUSTERED (Id),
        CONSTRAINT CK_COA_Type CHECK (Type IN (N'Assets', N'Liabilities', N'Equity', N'Revenue', N'Expense')),
        CONSTRAINT CK_COA_Level CHECK (Level BETWEEN 1 AND 6)
    );

    CREATE UNIQUE INDEX UQ_COA_TenantCode
        ON erp.ChartOfAccounts (TenantId, Code);

    CREATE INDEX IX_COA_TenantId
        ON erp.ChartOfAccounts (TenantId);

    CREATE INDEX IX_COA_TenantId_Code
        ON erp.ChartOfAccounts (TenantId, Code);

    CREATE INDEX IX_COA_ParentCode
        ON erp.ChartOfAccounts (ParentCode)
        WHERE ParentCode IS NOT NULL;
END
GO

-- ============================================================
-- TABLE: erp.AccountingPeriods
-- ============================================================

IF OBJECT_ID(N'erp.AccountingPeriods', N'U') IS NULL
BEGIN
    CREATE TABLE erp.AccountingPeriods (
        Id             INT          NOT NULL IDENTITY(1,1),
        TenantId       INT          NOT NULL,
        Month          TINYINT      NOT NULL,
        Year           SMALLINT     NOT NULL,
        IsLocked       BIT          NOT NULL CONSTRAINT DF_AP_IsLocked DEFAULT (0),
        LockedAt       DATETIME2(7) NULL,
        LockedByUserId INT          NULL,
        Notes          NVARCHAR(500) NULL,
        CreatedAt      DATETIME2(7) NOT NULL CONSTRAINT DF_AP_CreatedAt DEFAULT (SYSUTCDATETIME()),
        UpdatedAt      DATETIME2(7) NOT NULL CONSTRAINT DF_AP_UpdatedAt DEFAULT (SYSUTCDATETIME()),

        CONSTRAINT PK_AccountingPeriods PRIMARY KEY CLUSTERED (Id),
        CONSTRAINT FK_AP_Tenants FOREIGN KEY (TenantId)
            REFERENCES erp.Tenants (Id) ON DELETE CASCADE ON UPDATE NO ACTION,
        CONSTRAINT FK_AP_LockedByUser FOREIGN KEY (LockedByUserId)
            REFERENCES erp.Users (Id) ON DELETE NO ACTION ON UPDATE NO ACTION,
        CONSTRAINT UQ_AP_TenantMonthYear UNIQUE (TenantId, Month, Year),
        CONSTRAINT CK_AP_Month CHECK (Month BETWEEN 1 AND 12),
        CONSTRAINT CK_AP_Year CHECK (Year BETWEEN 2000 AND 2100),
        CONSTRAINT CK_AP_LockedConsistency CHECK (
            (IsLocked = 0 AND LockedAt IS NULL AND LockedByUserId IS NULL) OR
            (IsLocked = 1 AND LockedAt IS NOT NULL AND LockedByUserId IS NOT NULL)
        )
    );

    CREATE INDEX IX_AccountingPeriods_TenantId
        ON erp.AccountingPeriods (TenantId);

    CREATE INDEX IX_AccountingPeriods_TenantId_YearMonth
        ON erp.AccountingPeriods (TenantId, Year DESC, Month DESC);
END
GO

-- ============================================================
-- TABLE: erp.Invoices
-- ============================================================

IF OBJECT_ID(N'erp.Invoices', N'U') IS NULL
BEGIN
    CREATE TABLE erp.Invoices (
        Id                INT              NOT NULL IDENTITY(1,1),
        TenantId          INT              NOT NULL,
        Number            NVARCHAR(50)     NOT NULL,
        ClientId          INT              NOT NULL,
        IssueDate         DATE             NOT NULL,
        DueDate           DATE             NOT NULL,
        -- Status: Draft / Issued / Sent / PartiallyPaid / Paid / Cancelled / Overdue
        Status            NVARCHAR(20)     NOT NULL CONSTRAINT DF_Invoices_Status DEFAULT (N'Draft'),
        InvoiceType       NVARCHAR(20)     NOT NULL CONSTRAINT DF_Invoices_Type DEFAULT (N'Sales'),
        Currency          CHAR(3)          NOT NULL CONSTRAINT DF_Invoices_Currency DEFAULT ('RSD'),
        Amount            DECIMAL(18,2)    NOT NULL CONSTRAINT DF_Invoices_Amount DEFAULT (0),
        TaxAmount         DECIMAL(18,2)    NOT NULL CONSTRAINT DF_Invoices_TaxAmount DEFAULT (0),
        TotalAmount       DECIMAL(18,2)    NOT NULL CONSTRAINT DF_Invoices_TotalAmount DEFAULT (0),
        PaidAmount        DECIMAL(18,2)    NOT NULL CONSTRAINT DF_Invoices_PaidAmount DEFAULT (0),
        AccountingPeriodId INT             NULL,
        Notes             NVARCHAR(MAX)    NULL,
        IntegrityHash     NVARCHAR(64)     NULL,   -- SHA-256 hex of canonical fields
        CreatedAt         DATETIME2(7)     NOT NULL CONSTRAINT DF_Invoices_CreatedAt DEFAULT (SYSUTCDATETIME()),
        UpdatedAt         DATETIME2(7)     NOT NULL CONSTRAINT DF_Invoices_UpdatedAt DEFAULT (SYSUTCDATETIME()),
        CreatedByUserId   INT              NOT NULL,

        CONSTRAINT PK_Invoices PRIMARY KEY CLUSTERED (Id),
        CONSTRAINT FK_Invoices_Tenants FOREIGN KEY (TenantId)
            REFERENCES erp.Tenants (Id) ON DELETE CASCADE ON UPDATE NO ACTION,
        CONSTRAINT FK_Invoices_Clients FOREIGN KEY (ClientId)
            REFERENCES erp.Clients (Id) ON DELETE NO ACTION ON UPDATE NO ACTION,
        CONSTRAINT FK_Invoices_AccountingPeriods FOREIGN KEY (AccountingPeriodId)
            REFERENCES erp.AccountingPeriods (Id) ON DELETE NO ACTION ON UPDATE NO ACTION,
        CONSTRAINT FK_Invoices_CreatedBy FOREIGN KEY (CreatedByUserId)
            REFERENCES erp.Users (Id) ON DELETE NO ACTION ON UPDATE NO ACTION,
        CONSTRAINT CK_Invoices_Status CHECK (Status IN (
            N'Draft', N'Issued', N'Sent', N'PartiallyPaid', N'Paid', N'Cancelled', N'Overdue'
        )),
        CONSTRAINT CK_Invoices_Type CHECK (InvoiceType IN (N'Sales', N'Purchase', N'CreditNote', N'DebitNote')),
        CONSTRAINT CK_Invoices_DueDate CHECK (DueDate >= IssueDate),
        CONSTRAINT CK_Invoices_Amounts CHECK (
            Amount >= 0 AND TaxAmount >= 0 AND TotalAmount >= 0 AND PaidAmount >= 0
        ),
        CONSTRAINT CK_Invoices_TotalAmount CHECK (
            TotalAmount = Amount + TaxAmount
        )
    );

    CREATE UNIQUE INDEX UQ_Invoices_TenantNumber
        ON erp.Invoices (TenantId, Number);

    CREATE INDEX IX_Invoices_TenantId
        ON erp.Invoices (TenantId);

    CREATE INDEX IX_Invoices_TenantId_Status
        ON erp.Invoices (TenantId, Status);

    CREATE INDEX IX_Invoices_TenantId_IssueDate
        ON erp.Invoices (TenantId, IssueDate DESC);

    CREATE INDEX IX_Invoices_ClientId
        ON erp.Invoices (ClientId);

    CREATE INDEX IX_Invoices_AccountingPeriodId
        ON erp.Invoices (AccountingPeriodId)
        WHERE AccountingPeriodId IS NOT NULL;
END
GO

-- ============================================================
-- TABLE: erp.InvoiceItems
-- ============================================================

IF OBJECT_ID(N'erp.InvoiceItems', N'U') IS NULL
BEGIN
    CREATE TABLE erp.InvoiceItems (
        Id                  INT              NOT NULL IDENTITY(1,1),
        InvoiceId           INT              NOT NULL,
        LineNumber          SMALLINT         NOT NULL CONSTRAINT DF_InvItems_LineNo DEFAULT (1),
        Description         NVARCHAR(500)    NOT NULL,
        Quantity            DECIMAL(18,4)    NOT NULL CONSTRAINT DF_InvItems_Qty DEFAULT (1),
        Unit                NVARCHAR(20)     NOT NULL CONSTRAINT DF_InvItems_Unit DEFAULT (N'kom'),
        UnitPriceAmount     DECIMAL(18,4)    NOT NULL,
        UnitPriceCurrency   CHAR(3)          NOT NULL CONSTRAINT DF_InvItems_Currency DEFAULT ('RSD'),
        VatRatePercent      DECIMAL(5,2)     NOT NULL CONSTRAINT DF_InvItems_VatRate DEFAULT (20),
        DiscountPercent     DECIMAL(5,2)     NOT NULL CONSTRAINT DF_InvItems_Discount DEFAULT (0),
        NetAmount           AS (CAST(ROUND(Quantity * UnitPriceAmount * (1 - DiscountPercent / 100.0), 2) AS DECIMAL(18,2))) PERSISTED,
        VatAmount           AS (CAST(ROUND(Quantity * UnitPriceAmount * (1 - DiscountPercent / 100.0) * VatRatePercent / 100.0, 2) AS DECIMAL(18,2))) PERSISTED,
        AccountCode         VARCHAR(10)      NULL,  -- revenue/expense account reference

        CONSTRAINT PK_InvoiceItems PRIMARY KEY CLUSTERED (Id),
        CONSTRAINT FK_InvoiceItems_Invoices FOREIGN KEY (InvoiceId)
            REFERENCES erp.Invoices (Id) ON DELETE CASCADE ON UPDATE NO ACTION,
        CONSTRAINT CK_InvItems_Quantity CHECK (Quantity > 0),
        CONSTRAINT CK_InvItems_UnitPrice CHECK (UnitPriceAmount >= 0),
        CONSTRAINT CK_InvItems_VatRate CHECK (VatRatePercent IN (0, 10, 20)),
        CONSTRAINT CK_InvItems_Discount CHECK (DiscountPercent BETWEEN 0 AND 100),
        CONSTRAINT CK_InvItems_LineNumber CHECK (LineNumber > 0)
    );

    CREATE INDEX IX_InvoiceItems_InvoiceId
        ON erp.InvoiceItems (InvoiceId);
END
GO

-- ============================================================
-- TABLE: erp.JournalEntries
-- ============================================================

IF OBJECT_ID(N'erp.JournalEntries', N'U') IS NULL
BEGIN
    CREATE TABLE erp.JournalEntries (
        Id                 INT              NOT NULL IDENTITY(1,1),
        TenantId           INT              NOT NULL,
        Number             NVARCHAR(50)     NOT NULL,
        Date               DATE             NOT NULL,
        Description        NVARCHAR(500)    NOT NULL,
        -- Status: Draft / Posted / Reversed / Voided
        Status             NVARCHAR(20)     NOT NULL CONSTRAINT DF_JE_Status DEFAULT (N'Draft'),
        TotalDebitAmount   DECIMAL(18,2)    NOT NULL CONSTRAINT DF_JE_Debit DEFAULT (0),
        TotalCreditAmount  DECIMAL(18,2)    NOT NULL CONSTRAINT DF_JE_Credit DEFAULT (0),
        Currency           CHAR(3)          NOT NULL CONSTRAINT DF_JE_Currency DEFAULT ('RSD'),
        IntegrityHash      NVARCHAR(64)     NULL,   -- SHA-256 of entry content
        PreviousHash       NVARCHAR(64)     NULL,   -- hash-chain linkage
        PostedByUserId     INT              NULL,
        PostedAtUtc        DATETIME2(7)     NULL,
        SourceType         NVARCHAR(50)     NULL,   -- 'Invoice', 'Manual', 'Payroll', etc.
        SourceId           INT              NULL,   -- FK to source entity Id
        AccountingPeriodId INT              NULL,
        CreatedAt          DATETIME2(7)     NOT NULL CONSTRAINT DF_JE_CreatedAt DEFAULT (SYSUTCDATETIME()),
        UpdatedAt          DATETIME2(7)     NOT NULL CONSTRAINT DF_JE_UpdatedAt DEFAULT (SYSUTCDATETIME()),

        CONSTRAINT PK_JournalEntries PRIMARY KEY CLUSTERED (Id),
        CONSTRAINT FK_JE_Tenants FOREIGN KEY (TenantId)
            REFERENCES erp.Tenants (Id) ON DELETE CASCADE ON UPDATE NO ACTION,
        CONSTRAINT FK_JE_PostedBy FOREIGN KEY (PostedByUserId)
            REFERENCES erp.Users (Id) ON DELETE NO ACTION ON UPDATE NO ACTION,
        CONSTRAINT FK_JE_AccountingPeriod FOREIGN KEY (AccountingPeriodId)
            REFERENCES erp.AccountingPeriods (Id) ON DELETE NO ACTION ON UPDATE NO ACTION,
        CONSTRAINT CK_JE_Status CHECK (Status IN (N'Draft', N'Posted', N'Reversed', N'Voided')),
        CONSTRAINT CK_JE_BalancedEntry CHECK (
            Status = N'Draft' OR TotalDebitAmount = TotalCreditAmount
        ),
        CONSTRAINT CK_JE_Amounts CHECK (TotalDebitAmount >= 0 AND TotalCreditAmount >= 0),
        CONSTRAINT CK_JE_PostedConsistency CHECK (
            (Status != N'Posted') OR
            (Status = N'Posted' AND PostedByUserId IS NOT NULL AND PostedAtUtc IS NOT NULL)
        )
    );

    CREATE UNIQUE INDEX UQ_JE_TenantNumber
        ON erp.JournalEntries (TenantId, Number);

    CREATE INDEX IX_JE_TenantId
        ON erp.JournalEntries (TenantId);

    CREATE INDEX IX_JE_TenantId_Date
        ON erp.JournalEntries (TenantId, Date DESC);

    CREATE INDEX IX_JE_TenantId_Status
        ON erp.JournalEntries (TenantId, Status);

    CREATE INDEX IX_JE_SourceType_SourceId
        ON erp.JournalEntries (SourceType, SourceId)
        WHERE SourceId IS NOT NULL;
END
GO

-- ============================================================
-- TABLE: erp.JournalLines
-- ============================================================

IF OBJECT_ID(N'erp.JournalLines', N'U') IS NULL
BEGIN
    CREATE TABLE erp.JournalLines (
        Id               INT              NOT NULL IDENTITY(1,1),
        JournalEntryId   INT              NOT NULL,
        LineNumber       SMALLINT         NOT NULL,
        AccountId        INT              NOT NULL,  -- FK to ChartOfAccounts
        DebitAmount      DECIMAL(18,2)    NOT NULL CONSTRAINT DF_JL_Debit DEFAULT (0),
        DebitCurrency    CHAR(3)          NOT NULL CONSTRAINT DF_JL_DebitCur DEFAULT ('RSD'),
        CreditAmount     DECIMAL(18,2)    NOT NULL CONSTRAINT DF_JL_Credit DEFAULT (0),
        CreditCurrency   CHAR(3)          NOT NULL CONSTRAINT DF_JL_CreditCur DEFAULT ('RSD'),
        Note             NVARCHAR(500)    NULL,

        CONSTRAINT PK_JournalLines PRIMARY KEY CLUSTERED (Id),
        CONSTRAINT FK_JL_JournalEntries FOREIGN KEY (JournalEntryId)
            REFERENCES erp.JournalEntries (Id) ON DELETE CASCADE ON UPDATE NO ACTION,
        CONSTRAINT FK_JL_ChartOfAccounts FOREIGN KEY (AccountId)
            REFERENCES erp.ChartOfAccounts (Id) ON DELETE NO ACTION ON UPDATE NO ACTION,
        CONSTRAINT CK_JL_NotBothSides CHECK (
            NOT (DebitAmount > 0 AND CreditAmount > 0)
        ),
        CONSTRAINT CK_JL_AtLeastOneSide CHECK (
            DebitAmount > 0 OR CreditAmount > 0
        ),
        CONSTRAINT CK_JL_Amounts CHECK (DebitAmount >= 0 AND CreditAmount >= 0),
        CONSTRAINT CK_JL_LineNumber CHECK (LineNumber > 0)
    );

    CREATE INDEX IX_JL_JournalEntryId
        ON erp.JournalLines (JournalEntryId);

    CREATE INDEX IX_JL_AccountId
        ON erp.JournalLines (AccountId);
END
GO

-- ============================================================
-- TABLE: erp.Employees
-- Sensitive fields: JMBG stored only as SHA-256 hash (pseudonymization)
-- PII fields (BirthDate, Salary, BankAccount) encrypted at rest via
-- column-level encryption or application-side AES-256-GCM.
-- The *Encrypted columns store Base64(IV || CipherText || Tag).
-- ============================================================

IF OBJECT_ID(N'erp.Employees', N'U') IS NULL
BEGIN
    CREATE TABLE erp.Employees (
        Id                        INT              NOT NULL IDENTITY(1,1),
        TenantId                  INT              NOT NULL,
        EmployeeNumber            NVARCHAR(30)     NOT NULL,
        FirstName                 NVARCHAR(100)    NOT NULL,
        LastName                  NVARCHAR(100)    NOT NULL,
        MiddleName                NVARCHAR(100)    NULL,
        -- JMBG is pseudonymized: only SHA-256 hash stored in DB
        JMBGHashSha256            CHAR(64)         NULL,
        -- Encrypted PII fields (application-side AES-256-GCM, Base64 encoded)
        BirthDateEncrypted        NVARCHAR(256)    NULL,
        BankAccountEncrypted      NVARCHAR(256)    NULL,   -- tekući račun
        -- Employment info
        Position                  NVARCHAR(200)    NULL,
        Department                NVARCHAR(100)    NULL,
        EmploymentType            NVARCHAR(30)     NOT NULL CONSTRAINT DF_Emp_EmpType DEFAULT (N'FullTime'),
        ContractType              NVARCHAR(30)     NOT NULL CONSTRAINT DF_Emp_ContractType DEFAULT (N'Indefinite'),
        StartDate                 DATE             NOT NULL,
        EndDate                   DATE             NULL,
        IsActive                  BIT              NOT NULL CONSTRAINT DF_Emp_IsActive DEFAULT (1),
        -- Salary (encrypted)
        GrossSalaryEncrypted      NVARCHAR(256)    NULL,
        NetSalaryEncrypted        NVARCHAR(256)    NULL,
        -- Tax & contributions
        TaxExemptionAmount        DECIMAL(18,2)    NOT NULL CONSTRAINT DF_Emp_TaxExempt DEFAULT (0),
        -- Contact
        Email                     NVARCHAR(254)    NULL,
        Phone                     NVARCHAR(50)     NULL,
        Address                   NVARCHAR(300)    NULL,
        City                      NVARCHAR(100)    NULL,
        -- Manager
        ManagerId                 INT              NULL,
        -- Metadata
        Notes                     NVARCHAR(MAX)    NULL,
        DataRetentionDeleteAt     DATE             NULL,   -- GDPR / ZZPL schedule
        CreatedAt                 DATETIME2(7)     NOT NULL CONSTRAINT DF_Emp_CreatedAt DEFAULT (SYSUTCDATETIME()),
        UpdatedAt                 DATETIME2(7)     NOT NULL CONSTRAINT DF_Emp_UpdatedAt DEFAULT (SYSUTCDATETIME()),
        CreatedByUserId           INT              NOT NULL,

        CONSTRAINT PK_Employees PRIMARY KEY CLUSTERED (Id),
        CONSTRAINT FK_Employees_Tenants FOREIGN KEY (TenantId)
            REFERENCES erp.Tenants (Id) ON DELETE CASCADE ON UPDATE NO ACTION,
        CONSTRAINT FK_Employees_Manager FOREIGN KEY (ManagerId)
            REFERENCES erp.Employees (Id) ON DELETE NO ACTION ON UPDATE NO ACTION,
        CONSTRAINT FK_Employees_CreatedBy FOREIGN KEY (CreatedByUserId)
            REFERENCES erp.Users (Id) ON DELETE NO ACTION ON UPDATE NO ACTION,
        CONSTRAINT UQ_Employees_TenantNumber UNIQUE (TenantId, EmployeeNumber),
        CONSTRAINT CK_Employees_EmploymentType CHECK (EmploymentType IN (
            N'FullTime', N'PartTime', N'Contract', N'Internship', N'Retired'
        )),
        CONSTRAINT CK_Employees_ContractType CHECK (ContractType IN (
            N'Indefinite', N'FixedTerm', N'Probation', N'Seasonal'
        )),
        CONSTRAINT CK_Employees_EndDate CHECK (EndDate IS NULL OR EndDate >= StartDate),
        CONSTRAINT CK_Employees_TaxExempt CHECK (TaxExemptionAmount >= 0),
        CONSTRAINT CK_Employees_JMBG CHECK (
            JMBGHashSha256 IS NULL OR LEN(JMBGHashSha256) = 64
        )
    );

    -- Key lookup: find employee by JMBG hash within tenant
    CREATE INDEX IX_Employees_TenantId_JMBGHash
        ON erp.Employees (TenantId, JMBGHashSha256)
        WHERE JMBGHashSha256 IS NOT NULL;

    CREATE INDEX IX_Employees_TenantId
        ON erp.Employees (TenantId);

    CREATE INDEX IX_Employees_TenantId_IsActive
        ON erp.Employees (TenantId, IsActive);

    CREATE INDEX IX_Employees_ManagerId
        ON erp.Employees (ManagerId)
        WHERE ManagerId IS NOT NULL;
END
GO

-- ============================================================
-- TABLE: erp.AuditLog
-- Append-only. No UPDATE or DELETE permissions should be granted.
-- ============================================================

IF OBJECT_ID(N'erp.AuditLog', N'U') IS NULL
BEGIN
    CREATE TABLE erp.AuditLog (
        Id           BIGINT           NOT NULL IDENTITY(1,1),
        TenantId     INT              NOT NULL,
        UserId       INT              NULL,
        Action       NVARCHAR(50)     NOT NULL,       -- 'Create','Update','Delete','Login','Logout','Lock',etc.
        EntityType   NVARCHAR(100)    NOT NULL,
        EntityId     NVARCHAR(50)     NULL,
        OldValues    NVARCHAR(MAX)    NULL,            -- JSON snapshot
        NewValues    NVARCHAR(MAX)    NULL,            -- JSON snapshot
        Timestamp    DATETIME2(7)     NOT NULL CONSTRAINT DF_Audit_Timestamp DEFAULT (SYSUTCDATETIME()),
        IpAddress    VARCHAR(45)      NULL,            -- IPv4 or IPv6
        UserAgent    NVARCHAR(500)    NULL,
        CorrelationId UNIQUEIDENTIFIER NULL,

        CONSTRAINT PK_AuditLog PRIMARY KEY CLUSTERED (Id),
        CONSTRAINT CK_Audit_Action CHECK (Action IN (
            N'Create', N'Update', N'Delete',
            N'Login', N'Logout', N'LoginFailed',
            N'Lock', N'Unlock', N'Post', N'Reverse',
            N'Export', N'Import', N'PasswordChange', N'RoleChange'
        )),
        CONSTRAINT CK_Audit_JsonOld CHECK (OldValues IS NULL OR ISJSON(OldValues) = 1),
        CONSTRAINT CK_Audit_JsonNew CHECK (NewValues IS NULL OR ISJSON(NewValues) = 1)
    );

    CREATE INDEX IX_AuditLog_TenantId
        ON erp.AuditLog (TenantId);

    CREATE INDEX IX_AuditLog_TenantId_Timestamp
        ON erp.AuditLog (TenantId, Timestamp DESC);

    CREATE INDEX IX_AuditLog_EntityType_EntityId
        ON erp.AuditLog (EntityType, EntityId)
        WHERE EntityId IS NOT NULL;

    CREATE INDEX IX_AuditLog_UserId
        ON erp.AuditLog (UserId)
        WHERE UserId IS NOT NULL;
END
GO

-- ============================================================
-- DENY destructive permissions on AuditLog (run as sysadmin)
-- ============================================================

-- CREATE USER [erp_app] FOR LOGIN [erp_app];  -- uncomment and adapt
-- GRANT SELECT, INSERT ON erp.AuditLog TO [erp_app];
-- DENY UPDATE, DELETE ON erp.AuditLog TO [erp_app];

-- ============================================================
-- Useful views
-- ============================================================

GO
CREATE OR ALTER VIEW erp.vw_InvoiceSummary AS
SELECT
    i.Id,
    i.TenantId,
    i.Number,
    i.IssueDate,
    i.DueDate,
    i.Status,
    i.InvoiceType,
    i.Currency,
    i.Amount,
    i.TaxAmount,
    i.TotalAmount,
    i.PaidAmount,
    i.TotalAmount - i.PaidAmount       AS BalanceDue,
    c.Name                             AS ClientName,
    c.PIB                              AS ClientPIB,
    CASE
        WHEN i.DueDate < CAST(GETUTCDATE() AS DATE)
         AND i.Status NOT IN (N'Paid', N'Cancelled') THEN 1
        ELSE 0
    END                                AS IsOverdue,
    DATEDIFF(DAY, i.DueDate, CAST(GETUTCDATE() AS DATE)) AS DaysOverdue
FROM erp.Invoices i
INNER JOIN erp.Clients c ON c.Id = i.ClientId;
GO

CREATE OR ALTER VIEW erp.vw_TrialBalance AS
SELECT
    jl.AccountId,
    coa.Code        AS AccountCode,
    coa.Name        AS AccountName,
    coa.Type        AS AccountType,
    je.TenantId,
    SUM(jl.DebitAmount)  AS TotalDebit,
    SUM(jl.CreditAmount) AS TotalCredit,
    SUM(jl.DebitAmount) - SUM(jl.CreditAmount) AS NetBalance
FROM erp.JournalLines jl
INNER JOIN erp.JournalEntries je ON je.Id = jl.JournalEntryId
INNER JOIN erp.ChartOfAccounts coa ON coa.Id = jl.AccountId
WHERE je.Status = N'Posted'
GROUP BY
    jl.AccountId,
    coa.Code,
    coa.Name,
    coa.Type,
    je.TenantId;
GO

PRINT 'Schema 001_schema.sql applied successfully.';
GO
