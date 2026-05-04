-- ============================================================
-- AccountingERP - Demo Tenant Seed Data
-- File: 003_seed_demo_tenant.sql
-- Purpose: Realistic demo data for development / QA
-- IMPORTANT: Do NOT run in production!
-- ============================================================

USE AccountingERP;
GO

SET NOCOUNT ON;
SET XACT_ABORT ON;
GO

BEGIN TRANSACTION;

-- Guard: skip if demo tenant already exists
IF EXISTS (SELECT 1 FROM erp.Tenants WHERE PIB = '123456789')
BEGIN
    PRINT 'Demo tenant already exists. Skipping seed.';
    ROLLBACK;
    RETURN;
END

-- ============================================================
-- 1. TENANT
-- ============================================================

DECLARE @TenantId INT;

INSERT INTO erp.Tenants
    (Name, PIB, MaticniBroj, Address, City, Country, Email, Phone, IsActive, SubscriptionPlan)
VALUES
    (N'Demo d.o.o. Beograd', '123456789', '12345678',
     N'Knez Mihailova 10', N'Beograd', N'Serbia',
     'office@demo-doo.rs', '+381 11 123 4567', 1, N'Professional');

SET @TenantId = SCOPE_IDENTITY();

PRINT N'Tenant created: Id=' + CAST(@TenantId AS NVARCHAR(10));

-- ============================================================
-- 2. USERS
-- ============================================================

DECLARE @AdminUserId INT;
DECLARE @AccountantUserId INT;
DECLARE @ViewerUserId INT;

-- Admin user
-- PasswordHash is BCrypt of "Admin@123!" (rounds=12) – placeholder for prod rotation
INSERT INTO erp.Users
    (TenantId, Username, Email, PasswordHash, Role, FirstName, LastName, IsActive, EmailConfirmed)
VALUES
    (@TenantId, 'admin', 'admin@demo-doo.rs',
     '$2a$12$PLACEHOLDER_ADMIN_HASH_REPLACE_IN_PRODUCTION_xxxxxxxxxxx',
     'Admin', N'Nikola', N'Petrović', 1, 1);

SET @AdminUserId = SCOPE_IDENTITY();

-- Accountant user
INSERT INTO erp.Users
    (TenantId, Username, Email, PasswordHash, Role, FirstName, LastName, IsActive, EmailConfirmed)
VALUES
    (@TenantId, 'accountant1', 'milica.jovic@demo-doo.rs',
     '$2a$12$PLACEHOLDER_ACCOUNTANT_HASH_REPLACE_IN_PRODUCTION_xxxxxxx',
     'Accountant', N'Milica', N'Jović', 1, 1);

SET @AccountantUserId = SCOPE_IDENTITY();

-- Viewer user (read-only)
INSERT INTO erp.Users
    (TenantId, Username, Email, PasswordHash, Role, FirstName, LastName, IsActive, EmailConfirmed)
VALUES
    (@TenantId, 'viewer1', 'pregled@demo-doo.rs',
     '$2a$12$PLACEHOLDER_VIEWER_HASH_REPLACE_IN_PRODUCTION_xxxxxxxxxxx',
     'Viewer', N'Stefan', N'Marković', 1, 1);

SET @ViewerUserId = SCOPE_IDENTITY();

PRINT N'Users created: Admin=' + CAST(@AdminUserId AS NVARCHAR(10)) +
      ', Accountant=' + CAST(@AccountantUserId AS NVARCHAR(10)) +
      ', Viewer=' + CAST(@ViewerUserId AS NVARCHAR(10));

-- ============================================================
-- 3. CLIENTS (kupci i dobavljači)
-- ============================================================

DECLARE @ClientId1 INT, @ClientId2 INT, @ClientId3 INT, @ClientId4 INT, @ClientId5 INT;

-- Customer 1 – large domestic customer
INSERT INTO erp.Clients
    (TenantId, Name, PIB, MaticniBroj, Address, City, Country, IBAN,
     Email, Phone, ContactPerson, ClientType, IsActive)
VALUES
    (@TenantId, N'Telekomunikacije Srbije a.d.', '101670560', '17162543',
     N'Takovska 2', N'Beograd', N'Serbia', 'RS35908500100000256734',
     'nabavka@telekom.rs', '+381 11 321 4567', N'Dragana Ilić', 'Customer', 1);
SET @ClientId1 = SCOPE_IDENTITY();

-- Customer 2 – medium domestic customer
INSERT INTO erp.Clients
    (TenantId, Name, PIB, MaticniBroj, Address, City, Country, IBAN,
     Email, Phone, ContactPerson, ClientType, IsActive)
VALUES
    (@TenantId, N'Privredna Banka Beograd d.o.o.', '100002194', '07031982',
     N'Jovana Marinkovića 2', N'Beograd', N'Serbia', 'RS35908500100000111222',
     'racunovodstvo@pbb.rs', '+381 11 222 3333', N'Ana Stanković', 'Customer', 1);
SET @ClientId2 = SCOPE_IDENTITY();

-- Customer 3 – foreign customer (EU)
INSERT INTO erp.Clients
    (TenantId, Name, PIB, MaticniBroj, Address, City, Country, IBAN,
     Email, Phone, ContactPerson, ClientType, IsActive)
VALUES
    (@TenantId, N'SAP Deutschland GmbH', NULL, NULL,
     N'Dietmar-Hopp-Allee 16', N'Walldorf', N'Germany', 'DE89370400440532013000',
     'accounts@sap-partner.de', '+49 6227 7 00000', N'Klaus Müller', 'Customer', 1);
SET @ClientId3 = SCOPE_IDENTITY();

-- Supplier 1 – domestic office supplies
INSERT INTO erp.Clients
    (TenantId, Name, PIB, MaticniBroj, Address, City, Country, IBAN,
     Email, Phone, ContactPerson, ClientType, IsActive)
VALUES
    (@TenantId, N'Papir i materijal d.o.o.', '102345678', '12765432',
     N'Bulevar Vojvode Mišića 37', N'Beograd', N'Serbia', 'RS35908500100000987654',
     'fakture@papir-materijal.rs', '+381 11 444 5678', N'Marija Vasić', 'Supplier', 1);
SET @ClientId4 = SCOPE_IDENTITY();

-- Supplier 2 – IT hardware, also a customer (Both type)
INSERT INTO erp.Clients
    (TenantId, Name, PIB, MaticniBroj, Address, City, Country, IBAN,
     Email, Phone, ContactPerson, ClientType, IsActive)
VALUES
    (@TenantId, N'TechSolutions NS d.o.o.', '103456789', '13876543',
     N'Bulevar Oslobođenja 12', N'Novi Sad', N'Serbia', 'RS35908500100000654321',
     'racunovodstvo@techsolutions.rs', '+381 21 555 6789', N'Vladimir Đorđević', 'Both', 1);
SET @ClientId5 = SCOPE_IDENTITY();

PRINT N'5 clients created.';

-- ============================================================
-- 4. ACCOUNTING PERIODS (2024 and 2025, all unlocked)
-- ============================================================

DECLARE @Month INT = 1;
WHILE @Month <= 12
BEGIN
    INSERT INTO erp.AccountingPeriods (TenantId, Month, Year, IsLocked)
    VALUES (@TenantId, @Month, 2024, 0);

    INSERT INTO erp.AccountingPeriods (TenantId, Month, Year, IsLocked)
    VALUES (@TenantId, @Month, 2025, 0);

    SET @Month = @Month + 1;
END

-- Lock Jan-Sep 2024 (past periods)
UPDATE erp.AccountingPeriods
SET    IsLocked       = 1,
       LockedAt       = CAST('2025-01-15T08:00:00' AS DATETIME2(7)),
       LockedByUserId = @AdminUserId
WHERE  TenantId = @TenantId
  AND  Year    = 2024
  AND  Month   BETWEEN 1 AND 9;

PRINT N'Accounting periods created for 2024-2025.';

-- ============================================================
-- Helpers: look up chart of account IDs
-- ============================================================

DECLARE @AccId_200 INT, @AccId_241 INT, @AccId_480 INT, @AccId_430 INT;
DECLARE @AccId_600 INT, @AccId_610 INT, @AccId_470 INT, @AccId_500 INT;
DECLARE @AccId_520 INT, @AccId_530 INT;
DECLARE @PeriodId_2024_10 INT, @PeriodId_2024_11 INT, @PeriodId_2024_12 INT;
DECLARE @PeriodId_2025_01 INT;

SELECT @AccId_200 = Id FROM erp.ChartOfAccounts WHERE TenantId = 0 AND Code = '200';
SELECT @AccId_241 = Id FROM erp.ChartOfAccounts WHERE TenantId = 0 AND Code = '241';
SELECT @AccId_480 = Id FROM erp.ChartOfAccounts WHERE TenantId = 0 AND Code = '480';
SELECT @AccId_430 = Id FROM erp.ChartOfAccounts WHERE TenantId = 0 AND Code = '430';
SELECT @AccId_600 = Id FROM erp.ChartOfAccounts WHERE TenantId = 0 AND Code = '600';
SELECT @AccId_610 = Id FROM erp.ChartOfAccounts WHERE TenantId = 0 AND Code = '610';
SELECT @AccId_470 = Id FROM erp.ChartOfAccounts WHERE TenantId = 0 AND Code = '470';
SELECT @AccId_500 = Id FROM erp.ChartOfAccounts WHERE TenantId = 0 AND Code = '500';
SELECT @AccId_520 = Id FROM erp.ChartOfAccounts WHERE TenantId = 0 AND Code = '520';
SELECT @AccId_530 = Id FROM erp.ChartOfAccounts WHERE TenantId = 0 AND Code = '530';

SELECT @PeriodId_2024_10 = Id FROM erp.AccountingPeriods WHERE TenantId = @TenantId AND Year = 2024 AND Month = 10;
SELECT @PeriodId_2024_11 = Id FROM erp.AccountingPeriods WHERE TenantId = @TenantId AND Year = 2024 AND Month = 11;
SELECT @PeriodId_2024_12 = Id FROM erp.AccountingPeriods WHERE TenantId = @TenantId AND Year = 2024 AND Month = 12;
SELECT @PeriodId_2025_01 = Id FROM erp.AccountingPeriods WHERE TenantId = @TenantId AND Year = 2025 AND Month = 1;

-- ============================================================
-- 5. INVOICES with items
-- ============================================================

DECLARE @InvoiceId1 INT, @InvoiceId2 INT, @InvoiceId3 INT;

-- Invoice 1: IT consulting services to Telekom, Oct 2024, paid
INSERT INTO erp.Invoices
    (TenantId, Number, ClientId, IssueDate, DueDate, Status, InvoiceType,
     Currency, Amount, TaxAmount, TotalAmount, PaidAmount,
     AccountingPeriodId, CreatedByUserId,
     IntegrityHash, Notes)
VALUES
    (@TenantId, 'INV-2024-0001', @ClientId1,
     '2024-10-01', '2024-10-31', N'Paid', N'Sales',
     'RSD', 250000.00, 50000.00, 300000.00, 300000.00,
     @PeriodId_2024_10, @AccountantUserId,
     'a1b2c3d4e5f6a1b2c3d4e5f6a1b2c3d4e5f6a1b2c3d4e5f6a1b2c3d4e5f6a1b2',
     N'IT konsultantske usluge – oktobar 2024.');

SET @InvoiceId1 = SCOPE_IDENTITY();

INSERT INTO erp.InvoiceItems
    (InvoiceId, LineNumber, Description, Quantity, Unit, UnitPriceAmount, UnitPriceCurrency, VatRatePercent, DiscountPercent, AccountCode)
VALUES
    (@InvoiceId1, 1, N'Razvoj i implementacija ERP modula', 80.00, N'h', 2500.00, 'RSD', 20, 0, '610'),
    (@InvoiceId1, 2, N'Projektni menadžment i koordinacija', 20.00, N'h', 2500.00, 'RSD', 20, 0, '610');

-- Invoice 2: Software licenses to PBB, Nov 2024, issued (unpaid)
INSERT INTO erp.Invoices
    (TenantId, Number, ClientId, IssueDate, DueDate, Status, InvoiceType,
     Currency, Amount, TaxAmount, TotalAmount, PaidAmount,
     AccountingPeriodId, CreatedByUserId,
     IntegrityHash, Notes)
VALUES
    (@TenantId, 'INV-2024-0002', @ClientId2,
     '2024-11-15', '2024-12-15', N'Issued', N'Sales',
     'RSD', 180000.00, 36000.00, 216000.00, 0.00,
     @PeriodId_2024_11, @AccountantUserId,
     'b2c3d4e5f6a1b2c3d4e5f6a1b2c3d4e5f6a1b2c3d4e5f6a1b2c3d4e5f6a1b2c3',
     N'Godišnje licence za softver – 2025.');

SET @InvoiceId2 = SCOPE_IDENTITY();

INSERT INTO erp.InvoiceItems
    (InvoiceId, LineNumber, Description, Quantity, Unit, UnitPriceAmount, UnitPriceCurrency, VatRatePercent, DiscountPercent, AccountCode)
VALUES
    (@InvoiceId2, 1, N'Licenca AccountingERP Pro – godišnja pretplata', 3.00, N'kom', 48000.00, 'RSD', 20, 0, '600'),
    (@InvoiceId2, 2, N'Obuka korisnika (3 sesije)',                      6.00, N'h',   5000.00, 'RSD', 20, 0, '610');

-- Invoice 3: Export services to SAP Germany, Dec 2024, partially paid (EUR → VAT 0%)
INSERT INTO erp.Invoices
    (TenantId, Number, ClientId, IssueDate, DueDate, Status, InvoiceType,
     Currency, Amount, TaxAmount, TotalAmount, PaidAmount,
     AccountingPeriodId, CreatedByUserId,
     IntegrityHash, Notes)
VALUES
    (@TenantId, 'INV-2024-0003', @ClientId3,
     '2024-12-01', '2024-12-31', N'PartiallyPaid', N'Sales',
     'EUR', 8500.00, 0.00, 8500.00, 4250.00,
     @PeriodId_2024_12, @AccountantUserId,
     'c3d4e5f6a1b2c3d4e5f6a1b2c3d4e5f6a1b2c3d4e5f6a1b2c3d4e5f6a1b2c3d4',
     N'Software development services – Q4 2024 (izvoz usluga, PDV 0%).');

SET @InvoiceId3 = SCOPE_IDENTITY();

INSERT INTO erp.InvoiceItems
    (InvoiceId, LineNumber, Description, Quantity, Unit, UnitPriceAmount, UnitPriceCurrency, VatRatePercent, DiscountPercent, AccountCode)
VALUES
    (@InvoiceId3, 1, N'Backend development – REST API integration',  120.00, N'h', 60.00, 'EUR', 0, 0, '601'),
    (@InvoiceId3, 2, N'Code review and QA',                           20.00, N'h', 50.00, 'EUR', 0, 0, '601'),
    (@InvoiceId3, 3, N'Technical documentation',                       10.00, N'h', 50.00, 'EUR', 0, 5, '601');

PRINT N'3 invoices with items created.';

-- ============================================================
-- 6. JOURNAL ENTRIES
-- ============================================================

DECLARE @JEId1 INT, @JEId2 INT, @JEId3 INT;

-- Journal Entry 1: Recording INV-2024-0001 (sales invoice – revenue recognition)
INSERT INTO erp.JournalEntries
    (TenantId, Number, Date, Description, Status,
     TotalDebitAmount, TotalCreditAmount, Currency,
     IntegrityHash, PreviousHash,
     PostedByUserId, PostedAtUtc,
     SourceType, SourceId, AccountingPeriodId)
VALUES
    (@TenantId, 'KN-2024-0001', '2024-10-01',
     N'Knjiženje fakture INV-2024-0001 – prihod od usluga', N'Posted',
     300000.00, 300000.00, 'RSD',
     'd4e5f6a1b2c3d4e5f6a1b2c3d4e5f6a1b2c3d4e5f6a1b2c3d4e5f6a1b2c3d4e5',
     '0000000000000000000000000000000000000000000000000000000000000000',
     @AccountantUserId, '2024-10-02T07:30:00',
     N'Invoice', @InvoiceId1, @PeriodId_2024_10);

SET @JEId1 = SCOPE_IDENTITY();

-- Debit: Potraživanja od kupaca (200), Credit: Prihodi od usluga (610), PDV (480)
INSERT INTO erp.JournalLines
    (JournalEntryId, LineNumber, AccountId, DebitAmount, DebitCurrency, CreditAmount, CreditCurrency, Note)
VALUES
    (@JEId1, 1, @AccId_200, 300000.00, 'RSD', 0.00, 'RSD', N'Potraživanje – INV-2024-0001'),
    (@JEId1, 2, @AccId_610, 0.00, 'RSD', 250000.00, 'RSD', N'Prihod od usluga'),
    (@JEId1, 3, @AccId_480, 0.00, 'RSD', 50000.00, 'RSD', N'PDV izlazni 20%');

-- Journal Entry 2: Salary expense for October 2024 (zarade)
INSERT INTO erp.JournalEntries
    (TenantId, Number, Date, Description, Status,
     TotalDebitAmount, TotalCreditAmount, Currency,
     IntegrityHash, PreviousHash,
     PostedByUserId, PostedAtUtc,
     SourceType, SourceId, AccountingPeriodId)
VALUES
    (@TenantId, 'KN-2024-0002', '2024-10-31',
     N'Obračun zarada za oktobar 2024.', N'Posted',
     650000.00, 650000.00, 'RSD',
     'e5f6a1b2c3d4e5f6a1b2c3d4e5f6a1b2c3d4e5f6a1b2c3d4e5f6a1b2c3d4e5f6',
     'd4e5f6a1b2c3d4e5f6a1b2c3d4e5f6a1b2c3d4e5f6a1b2c3d4e5f6a1b2c3d4e5',
     @AccountantUserId, '2024-10-31T14:00:00',
     N'Payroll', NULL, @PeriodId_2024_10);

SET @JEId2 = SCOPE_IDENTITY();

INSERT INTO erp.JournalLines
    (JournalEntryId, LineNumber, AccountId, DebitAmount, DebitCurrency, CreditAmount, CreditCurrency, Note)
VALUES
    (@JEId2, 1, @AccId_520, 550000.00, 'RSD', 0.00, 'RSD', N'Troškovi bruto zarada – oktobar'),
    (@JEId2, 2, @AccId_530, 100000.00, 'RSD', 0.00, 'RSD', N'Doprinosi poslodavca – oktobar'),
    (@JEId2, 3, @AccId_470, 0.00, 'RSD', 650000.00, 'RSD', N'Obaveze za zarade – oktobar');

-- Journal Entry 3: Draft – prepaid office supplies (materijal)
INSERT INTO erp.JournalEntries
    (TenantId, Number, Date, Description, Status,
     TotalDebitAmount, TotalCreditAmount, Currency,
     IntegrityHash, PreviousHash,
     SourceType, AccountingPeriodId)
VALUES
    (@TenantId, 'KN-2025-0001', '2025-01-10',
     N'Nabavka kancelarijskog materijala – januar 2025.', N'Draft',
     0.00, 0.00, 'RSD',
     NULL, NULL,
     N'Manual', @PeriodId_2025_01);

SET @JEId3 = SCOPE_IDENTITY();

INSERT INTO erp.JournalLines
    (JournalEntryId, LineNumber, AccountId, DebitAmount, DebitCurrency, CreditAmount, CreditCurrency, Note)
VALUES
    (@JEId3, 1, @AccId_500, 18000.00, 'RSD', 0.00, 'RSD', N'Kancelarijski materijal'),
    (@JEId3, 2, @AccId_430, 0.00, 'RSD', 18000.00, 'RSD', N'Obaveza prema dobavljaču');

-- Update draft totals manually (would normally be done by application)
UPDATE erp.JournalEntries
SET TotalDebitAmount  = 18000.00,
    TotalCreditAmount = 18000.00
WHERE Id = @JEId3;

PRINT N'3 journal entries created.';

-- ============================================================
-- 7. EMPLOYEES (5 employees with diverse salary ranges)
-- All sensitive fields (salary, JMBG, birth date, bank account)
-- are placeholder-encrypted values prefixed with ENC: for demo.
-- In production these are AES-256-GCM ciphertext (Base64).
-- ============================================================

-- Employee 1: Senior Developer (Director-level salary)
INSERT INTO erp.Employees
    (TenantId, EmployeeNumber, FirstName, LastName, MiddleName,
     JMBGHashSha256,
     BirthDateEncrypted, BankAccountEncrypted,
     Position, Department, EmploymentType, ContractType,
     StartDate, EndDate, IsActive,
     GrossSalaryEncrypted, NetSalaryEncrypted,
     TaxExemptionAmount,
     Email, Phone, Address, City,
     ManagerId, CreatedByUserId)
VALUES
    (@TenantId, 'EMP-001', N'Nikola', N'Petrović', N'Aleksandar',
     'a665a45920422f9d417e4867efdc4fb8a04a1f3fff1fa07e998e86f7f7a27ae3',  -- SHA-256 of dummy JMBG
     'ENC:BASE64_PLACEHOLDER_BIRTHDATE_ADMIN',
     'ENC:BASE64_PLACEHOLDER_BANKACCOUNT_ADMIN',
     N'Direktor i suvlasnik', N'Menadžment', N'FullTime', N'Indefinite',
     '2018-06-01', NULL, 1,
     'ENC:BASE64_PLACEHOLDER_GROSS_250000',
     'ENC:BASE64_PLACEHOLDER_NET_172000',
     21712.00,
     'admin@demo-doo.rs', '+381 63 100 0001',
     N'Knez Mihailova 10', N'Beograd',
     NULL, @AdminUserId);

DECLARE @EmpId1 INT = SCOPE_IDENTITY();

-- Employee 2: Senior Accountant
INSERT INTO erp.Employees
    (TenantId, EmployeeNumber, FirstName, LastName, MiddleName,
     JMBGHashSha256,
     BirthDateEncrypted, BankAccountEncrypted,
     Position, Department, EmploymentType, ContractType,
     StartDate, EndDate, IsActive,
     GrossSalaryEncrypted, NetSalaryEncrypted,
     TaxExemptionAmount,
     Email, Phone, Address, City,
     ManagerId, CreatedByUserId)
VALUES
    (@TenantId, 'EMP-002', N'Milica', N'Jović', N'Dragana',
     'b3a8e0e1f9ab1busted2c3d4e5f6a1b2c3d4e5f6a1b2c3d4e5f6a1b2c3d4e5f6',
     'ENC:BASE64_PLACEHOLDER_BIRTHDATE_ACC',
     'ENC:BASE64_PLACEHOLDER_BANKACCOUNT_ACC',
     N'Viši računovođa', N'Finansije', N'FullTime', N'Indefinite',
     '2019-09-01', NULL, 1,
     'ENC:BASE64_PLACEHOLDER_GROSS_130000',
     'ENC:BASE64_PLACEHOLDER_NET_93000',
     21712.00,
     'milica.jovic@demo-doo.rs', '+381 63 200 0002',
     N'Studentski trg 5', N'Beograd',
     @EmpId1, @AdminUserId);

DECLARE @EmpId2 INT = SCOPE_IDENTITY();

-- Employee 3: Junior Developer
INSERT INTO erp.Employees
    (TenantId, EmployeeNumber, FirstName, LastName, MiddleName,
     JMBGHashSha256,
     BirthDateEncrypted, BankAccountEncrypted,
     Position, Department, EmploymentType, ContractType,
     StartDate, EndDate, IsActive,
     GrossSalaryEncrypted, NetSalaryEncrypted,
     TaxExemptionAmount,
     Email, Phone, Address, City,
     ManagerId, CreatedByUserId)
VALUES
    (@TenantId, 'EMP-003', N'Stefan', N'Marković', N'Bojan',
     'c4b9f1a2b3c4d5e6f7a8b9c0d1e2f3a4b5c6d7e8f9a0b1c2d3e4f5a6b7c8d9e0',
     'ENC:BASE64_PLACEHOLDER_BIRTHDATE_JR',
     'ENC:BASE64_PLACEHOLDER_BANKACCOUNT_JR',
     N'Junior developer', N'IT', N'FullTime', N'Indefinite',
     '2022-03-01', NULL, 1,
     'ENC:BASE64_PLACEHOLDER_GROSS_75000',
     'ENC:BASE64_PLACEHOLDER_NET_56000',
     21712.00,
     'pregled@demo-doo.rs', '+381 63 300 0003',
     N'Bulevar Kralja Aleksandra 55', N'Beograd',
     @EmpId1, @AdminUserId);

-- Employee 4: Part-time Marketing Assistant
INSERT INTO erp.Employees
    (TenantId, EmployeeNumber, FirstName, LastName, MiddleName,
     JMBGHashSha256,
     BirthDateEncrypted, BankAccountEncrypted,
     Position, Department, EmploymentType, ContractType,
     StartDate, EndDate, IsActive,
     GrossSalaryEncrypted, NetSalaryEncrypted,
     TaxExemptionAmount,
     Email, Phone, Address, City,
     ManagerId, CreatedByUserId)
VALUES
    (@TenantId, 'EMP-004', N'Ana', N'Nikolić', NULL,
     'd5ca3e4f5a6b7c8d9e0f1a2b3c4d5e6f7a8b9c0d1e2f3a4b5c6d7e8f9a0b1c2',
     'ENC:BASE64_PLACEHOLDER_BIRTHDATE_ANA',
     'ENC:BASE64_PLACEHOLDER_BANKACCOUNT_ANA',
     N'Marketing asistent', N'Marketing', N'PartTime', N'Indefinite',
     '2023-06-15', NULL, 1,
     'ENC:BASE64_PLACEHOLDER_GROSS_45000',
     'ENC:BASE64_PLACEHOLDER_NET_35000',
     21712.00,
     'ana.nikolic@demo-doo.rs', '+381 63 400 0004',
     N'Cara Lazara 18', N'Novi Sad',
     @EmpId1, @AdminUserId);

-- Employee 5: Fixed-term contract – Project Coordinator (high salary, ending soon)
INSERT INTO erp.Employees
    (TenantId, EmployeeNumber, FirstName, LastName, MiddleName,
     JMBGHashSha256,
     BirthDateEncrypted, BankAccountEncrypted,
     Position, Department, EmploymentType, ContractType,
     StartDate, EndDate, IsActive,
     GrossSalaryEncrypted, NetSalaryEncrypted,
     TaxExemptionAmount,
     Email, Phone, Address, City,
     Notes,
     ManagerId, CreatedByUserId,
     DataRetentionDeleteAt)
VALUES
    (@TenantId, 'EMP-005', N'Vladimir', N'Đorđević', N'Mihailo',
     'e6db3f5060a1b2c3d4e5f6a7b8c9d0e1f2a3b4c5d6e7f8a9b0c1d2e3f4a5b6c7',
     'ENC:BASE64_PLACEHOLDER_BIRTHDATE_VD',
     'ENC:BASE64_PLACEHOLDER_BANKACCOUNT_VD',
     N'Project Coordinator – EU fondovi', N'Projekti', N'FullTime', N'FixedTerm',
     '2024-01-01', '2025-12-31', 1,
     'ENC:BASE64_PLACEHOLDER_GROSS_185000',
     'ENC:BASE64_PLACEHOLDER_NET_130000',
     21712.00,
     'vladimir.djordjevic@demo-doo.rs', '+381 63 500 0005',
     N'Fruškogorska 7', N'Novi Sad',
     N'Ugovor na određeno – projekat EU digitalizacija. Produžetak za razmatranje Q3 2025.',
     @EmpId1, @AdminUserId,
     '2030-12-31');   -- GDPR data retention end

PRINT N'5 employees created.';

-- ============================================================
-- 8. AUDIT LOG – Initial entries
-- ============================================================

INSERT INTO erp.AuditLog
    (TenantId, UserId, Action, EntityType, EntityId, OldValues, NewValues, IpAddress)
VALUES
    (@TenantId, @AdminUserId, N'Create', N'Tenant',  CAST(@TenantId AS NVARCHAR(10)), NULL,
     N'{"name":"Demo d.o.o. Beograd","pib":"123456789"}', '127.0.0.1'),
    (@TenantId, @AdminUserId, N'Create', N'User',    CAST(@AdminUserId AS NVARCHAR(10)), NULL,
     N'{"username":"admin","role":"Admin"}', '127.0.0.1'),
    (@TenantId, @AdminUserId, N'Create', N'User',    CAST(@AccountantUserId AS NVARCHAR(10)), NULL,
     N'{"username":"accountant1","role":"Accountant"}', '127.0.0.1'),
    (@TenantId, @AccountantUserId, N'Post', N'JournalEntry', CAST(@JEId1 AS NVARCHAR(10)), NULL,
     N'{"number":"KN-2024-0001","totalDebit":300000,"totalCredit":300000}', '10.0.0.5'),
    (@TenantId, @AccountantUserId, N'Post', N'JournalEntry', CAST(@JEId2 AS NVARCHAR(10)), NULL,
     N'{"number":"KN-2024-0002","totalDebit":650000,"totalCredit":650000}', '10.0.0.5');

COMMIT TRANSACTION;

PRINT N'';
PRINT N'=== Demo tenant seed completed successfully ===';
PRINT N'TenantId  : ' + CAST(@TenantId AS NVARCHAR(10));
PRINT N'Admin user: admin / <set password via application>';
PRINT N'WARNING   : All PasswordHash fields are PLACEHOLDERS.';
PRINT N'            Run the application onboarding endpoint to set real passwords.';
GO
