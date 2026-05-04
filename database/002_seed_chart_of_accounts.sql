-- ============================================================
-- AccountingERP - Serbian Standard Chart of Accounts
-- File: 002_seed_chart_of_accounts.sql
-- Standard: Kontni plan Republike Srbije
-- Reference: Pravilnik o Kontnom Okviru (Sl. glasnik RS)
-- TenantId = 0 → system template accounts (all tenants inherit)
-- ============================================================

USE AccountingERP;
GO

SET NOCOUNT ON;
SET XACT_ABORT ON;
GO

BEGIN TRANSACTION;

-- Prevent double-seeding
IF EXISTS (SELECT 1 FROM erp.ChartOfAccounts WHERE TenantId = 0)
BEGIN
    PRINT 'Chart of accounts already seeded. Skipping.';
    ROLLBACK;
    RETURN;
END

-- ============================================================
-- KLASA 0 – Stalna imovina (Fixed Assets)
-- Type: Assets
-- ============================================================

-- Nematerijalna imovina (Intangible Assets)
INSERT INTO erp.ChartOfAccounts (TenantId, Code, Name, Type, ParentCode, Level) VALUES
(0, '00',   N'Nematerijalna imovina',                              'Assets', NULL,  1),
(0, '000',  N'Osnivački troškovi i razvojna istraživanja',         'Assets', '00',  2),
(0, '001',  N'Koncesije, patenti, licence i slična prava',         'Assets', '00',  2),
(0, '002',  N'Goodwill',                                           'Assets', '00',  2),
(0, '005',  N'Ulaganja u razvoj softvera',                         'Assets', '00',  2),
(0, '009',  N'Ispravka vrednosti nematerijalne imovine',           'Assets', '00',  2);

-- Nekretnine, postrojenja i oprema (Property, Plant and Equipment)
INSERT INTO erp.ChartOfAccounts (TenantId, Code, Name, Type, ParentCode, Level) VALUES
(0, '01',   N'Nekretnine, postrojenja i oprema',                   'Assets', NULL,  1),
(0, '010',  N'Nekretnine (zemljišta i objekti)',                   'Assets', '01',  2),
(0, '011',  N'Postrojenja i oprema',                               'Assets', '01',  2),
(0, '012',  N'Investicione nekretnine',                            'Assets', '01',  2),
(0, '013',  N'Ostala osnovna sredstva',                            'Assets', '01',  2),
(0, '014',  N'Osnovna sredstva u pripremi',                        'Assets', '01',  2),
(0, '015',  N'Avansna plaćanja za osnovna sredstva',               'Assets', '01',  2),
(0, '019',  N'Ispravka vrednosti nekretnina i opreme',             'Assets', '01',  2),
(0, '020',  N'Oprema',                                             'Assets', '01',  2),
(0, '029',  N'Ispravka vrednosti opreme',                          'Assets', '01',  2),
(0, '039',  N'Ispravka vrednosti ostalih stalnih sredstava',       'Assets', '01',  2);

-- Dugorocni finansijski plasmani (Long-term financial investments)
INSERT INTO erp.ChartOfAccounts (TenantId, Code, Name, Type, ParentCode, Level) VALUES
(0, '04',   N'Dugoročni finansijski plasmani',                     'Assets', NULL,  1),
(0, '040',  N'Učešća u kapitalu zavisnih pravnih lica',            'Assets', '04',  2),
(0, '041',  N'Učešća u kapitalu pridruženih pravnih lica',         'Assets', '04',  2),
(0, '043',  N'Dugoročne hartije od vrednosti',                     'Assets', '04',  2),
(0, '045',  N'Dati dugoročni zajmovi',                             'Assets', '04',  2);

-- ============================================================
-- KLASA 1 – Zalihe (Inventories)
-- Type: Assets
-- ============================================================

INSERT INTO erp.ChartOfAccounts (TenantId, Code, Name, Type, ParentCode, Level) VALUES
(0, '10',   N'Zalihe materijala i sirovina',                       'Assets', NULL,  1),
(0, '100',  N'Materijal i sirovine',                               'Assets', '10',  2),
(0, '101',  N'Materijal u obradi',                                 'Assets', '10',  2),
(0, '102',  N'Rezervni delovi',                                    'Assets', '10',  2),
(0, '103',  N'Sitni inventar',                                     'Assets', '10',  2),
(0, '109',  N'Ispravka vrednosti materijala',                      'Assets', '10',  2),
(0, '13',   N'Gotovi proizvodi',                                   'Assets', NULL,  1),
(0, '130',  N'Gotovi proizvodi',                                   'Assets', '13',  2),
(0, '131',  N'Nedovršena proizvodnja',                             'Assets', '13',  2),
(0, '14',   N'Roba',                                               'Assets', NULL,  1),
(0, '160',  N'Roba na zalihama',                                   'Assets', '14',  2),
(0, '161',  N'Roba u prometu',                                     'Assets', '14',  2);

-- ============================================================
-- KLASA 2 – Kratkoročna potraživanja, plasmani i gotovina
-- Type: Assets
-- ============================================================

INSERT INTO erp.ChartOfAccounts (TenantId, Code, Name, Type, ParentCode, Level) VALUES
(0, '20',   N'Kratkoročna potraživanja po osnovu prodaje',         'Assets', NULL,  1),
(0, '200',  N'Potraživanja od kupaca u zemlji',                    'Assets', '20',  2),
(0, '201',  N'Potraživanja od kupaca u inostranstvu',              'Assets', '20',  2),
(0, '202',  N'Menice i čekovi primljeni od kupaca',                'Assets', '20',  2),
(0, '204',  N'Potraživanja za PDV (prethodni porez)',              'Assets', '20',  2),
(0, '205',  N'Potraživanja za ostale poreze i doprinose',          'Assets', '20',  2),
(0, '209',  N'Ispravka vrednosti potraživanja od kupaca',          'Assets', '20',  2),
(0, '22',   N'Potraživanja iz specifičnih poslova',                'Assets', NULL,  1),
(0, '220',  N'Potraživanja od zaposlenih',                         'Assets', '22',  2),
(0, '221',  N'Potraživanja od ostalih',                            'Assets', '22',  2),
(0, '23',   N'Kratkoročni finansijski plasmani',                   'Assets', NULL,  1),
(0, '230',  N'Kratkoročni plasmani u zavisna pravna lica',         'Assets', '23',  2),
(0, '231',  N'Kratkoročni zajmovi i depoziti',                     'Assets', '23',  2),
(0, '24',   N'Gotovinski ekvivalenti i gotovina',                  'Assets', NULL,  1),
(0, '240',  N'Gotovinski ekvivalenti',                             'Assets', '24',  2),
(0, '241',  N'Tekući (poslovni) računi',                           'Assets', '24',  2),
(0, '242',  N'Blagajna',                                           'Assets', '24',  2),
(0, '243',  N'Devizni račun',                                      'Assets', '24',  2),
(0, '244',  N'Akreditivi',                                         'Assets', '24',  2),
(0, '27',   N'PDV i aktivna vremenska razgraničenja',              'Assets', NULL,  1),
(0, '270',  N'Unapred plaćeni troškovi',                           'Assets', '27',  2),
(0, '271',  N'Obračunati prihodi',                                 'Assets', '27',  2);

-- ============================================================
-- KLASA 3 – Kapital (Equity)
-- Type: Equity
-- ============================================================

INSERT INTO erp.ChartOfAccounts (TenantId, Code, Name, Type, ParentCode, Level) VALUES
(0, '30',   N'Osnovni kapital',                                    'Equity', NULL,  1),
(0, '300',  N'Akcijski kapital',                                   'Equity', '30',  2),
(0, '301',  N'Udeli u d.o.o.',                                     'Equity', '30',  2),
(0, '302',  N'Državni kapital',                                    'Equity', '30',  2),
(0, '303',  N'Zadružni udeli',                                     'Equity', '30',  2),
(0, '33',   N'Rezerve',                                            'Equity', NULL,  1),
(0, '330',  N'Zakonske rezerve',                                   'Equity', '33',  2),
(0, '331',  N'Statutarne i ostale rezerve',                        'Equity', '33',  2),
(0, '34',   N'Neraspoređeni dobitak / nepokriven gubitak',         'Equity', NULL,  1),
(0, '340',  N'Neraspoređeni dobitak ranijih godina',               'Equity', '34',  2),
(0, '341',  N'Neraspoređeni dobitak tekuće godine',                'Equity', '34',  2),
(0, '342',  N'Nepokriven gubitak ranijih godina',                  'Equity', '34',  2),
(0, '343',  N'Nepokriven gubitak tekuće godine',                   'Equity', '34',  2);

-- ============================================================
-- KLASA 4 – Dugoročne obaveze (Long-term Liabilities)
-- Type: Liabilities
-- ============================================================

INSERT INTO erp.ChartOfAccounts (TenantId, Code, Name, Type, ParentCode, Level) VALUES
(0, '40',   N'Dugoročne obaveze',                                  'Liabilities', NULL,  1),
(0, '400',  N'Dugoročni krediti od banaka u zemlji',               'Liabilities', '40',  2),
(0, '401',  N'Dugoročni krediti od banaka u inostranstvu',         'Liabilities', '40',  2),
(0, '403',  N'Dugoročne obaveze po emitovanim hartijama',          'Liabilities', '40',  2),
(0, '404',  N'Obaveze po finansijskom lizingu',                    'Liabilities', '40',  2),
(0, '41',   N'Dugoročna rezervisanja',                             'Liabilities', NULL,  1),
(0, '410',  N'Rezervisanja za troškove obnavljanja',               'Liabilities', '41',  2),
(0, '411',  N'Rezervisanja za sudske sporove',                     'Liabilities', '41',  2),
(0, '412',  N'Rezervisanja za otpremnine i jubilarne nagrade',     'Liabilities', '41',  2);

-- ============================================================
-- KLASA 5 – Kratkoročne obaveze (Short-term Liabilities)
-- Type: Liabilities
-- ============================================================

INSERT INTO erp.ChartOfAccounts (TenantId, Code, Name, Type, ParentCode, Level) VALUES
(0, '43',   N'Kratkoročne finansijske obaveze',                    'Liabilities', NULL,  1),
(0, '430',  N'Obaveze prema dobavljačima u zemlji',                'Liabilities', '43',  2),
(0, '431',  N'Obaveze prema dobavljačima u inostranstvu',          'Liabilities', '43',  2),
(0, '432',  N'Primljeni avansi, depoziti i kaucije',               'Liabilities', '43',  2),
(0, '433',  N'Obaveze za neto zarade i naknade',                   'Liabilities', '43',  2),
(0, '46',   N'Kratkoročni krediti i zajmovi',                      'Liabilities', NULL,  1),
(0, '460',  N'Kratkoročni krediti od banaka',                      'Liabilities', '46',  2),
(0, '461',  N'Deo dugoročnih kredita koji dospeva u kratkom roku', 'Liabilities', '46',  2),
(0, '47',   N'Obaveze za zarade i poreze',                         'Liabilities', NULL,  1),
(0, '470',  N'Obaveze za neto zarade',                             'Liabilities', '47',  2),
(0, '471',  N'Obaveze za porez na zarade (porez iz zarade)',       'Liabilities', '47',  2),
(0, '472',  N'Obaveze za PIO (penzijsko-invalidsko osiguranje)',   'Liabilities', '47',  2),
(0, '473',  N'Obaveze za zdravstveno osiguranje',                  'Liabilities', '47',  2),
(0, '474',  N'Obaveze za osiguranje za slučaj nezaposlenosti',     'Liabilities', '47',  2),
(0, '48',   N'Obaveze za PDV i ostale poreze',                     'Liabilities', NULL,  1),
(0, '480',  N'Obaveze za PDV (izlazni PDV)',                       'Liabilities', '48',  2),
(0, '481',  N'Obaveze za porez na dobit',                          'Liabilities', '48',  2),
(0, '482',  N'Obaveze za ostale poreze i takse',                   'Liabilities', '48',  2),
(0, '49',   N'Ostale kratkoročne obaveze',                         'Liabilities', NULL,  1),
(0, '490',  N'Ostale kratkoročne obaveze',                         'Liabilities', '49',  2),
(0, '491',  N'Obaveze za dividende i učešće u dobitku',            'Liabilities', '49',  2),
(0, '495',  N'Pasivna vremenska razgraničenja',                    'Liabilities', '49',  2);

-- ============================================================
-- KLASA 5/6 – Troškovi (Costs / Expenses)
-- Type: Expense
-- (Serbian kontni plan uses class 5 for costs of production,
--  class 6 for period costs. Both map to Expense type.)
-- ============================================================

INSERT INTO erp.ChartOfAccounts (TenantId, Code, Name, Type, ParentCode, Level) VALUES
(0, '50',   N'Troškovi materijala',                                'Expense', NULL,  1),
(0, '500',  N'Troškovi materijala i sirovina',                     'Expense', '50',  2),
(0, '501',  N'Troškovi pomoćnog materijala',                       'Expense', '50',  2),
(0, '502',  N'Troškovi ambalaže',                                  'Expense', '50',  2),
(0, '510',  N'Troškovi goriva i energije',                         'Expense', '50',  2),
(0, '511',  N'Troškovi električne energije',                       'Expense', '50',  2),
(0, '512',  N'Troškovi komunalnih usluga',                         'Expense', '50',  2),
(0, '52',   N'Troškovi zarada i naknada',                          'Expense', NULL,  1),
(0, '520',  N'Troškovi bruto zarada',                              'Expense', '52',  2),
(0, '521',  N'Troškovi naknada zarada',                            'Expense', '52',  2),
(0, '53',   N'Troškovi doprinosa na teret poslodavca',             'Expense', NULL,  1),
(0, '530',  N'Doprinosi za PIO na teret poslodavca',               'Expense', '53',  2),
(0, '531',  N'Doprinosi za zdravstvo na teret poslodavca',         'Expense', '53',  2),
(0, '532',  N'Doprinosi za nezaposlenost na teret poslodavca',     'Expense', '53',  2),
(0, '54',   N'Troškovi amortizacije i rezervisanja',               'Expense', NULL,  1),
(0, '540',  N'Troškovi amortizacije',                              'Expense', '54',  2),
(0, '541',  N'Troškovi rezervisanja',                              'Expense', '54',  2),
(0, '55',   N'Ostali poslovni rashodi',                            'Expense', NULL,  1),
(0, '550',  N'Troškovi usluga (zakup, IT, marketing...)',          'Expense', '55',  2),
(0, '551',  N'Troškovi reprezentacije',                            'Expense', '55',  2),
(0, '552',  N'Troškovi službenih putovanja',                       'Expense', '55',  2),
(0, '553',  N'Troškovi osiguranja',                                'Expense', '55',  2),
(0, '554',  N'Bankarske naknade i provizije',                      'Expense', '55',  2),
(0, '558',  N'Ostali troškovi poslovanja',                         'Expense', '55',  2),
(0, '56',   N'Finansijski rashodi',                                'Expense', NULL,  1),
(0, '560',  N'Rashodi kamata',                                     'Expense', '56',  2),
(0, '561',  N'Negativne kursne razlike',                           'Expense', '56',  2),
(0, '562',  N'Ostali finansijski rashodi',                         'Expense', '56',  2),
(0, '57',   N'Ostali rashodi',                                     'Expense', NULL,  1),
(0, '570',  N'Rashodi od prodaje stalne imovine',                  'Expense', '57',  2),
(0, '571',  N'Rashodi od rashodovanja imovine',                    'Expense', '57',  2),
(0, '58',   N'Porez na dobit',                                     'Expense', NULL,  1),
(0, '580',  N'Tekući porez na dobit',                              'Expense', '58',  2),
(0, '581',  N'Odložene poreske obaveze / potraživanja',            'Expense', '58',  2);

-- ============================================================
-- KLASA 6/7 – Prihodi (Revenue)
-- Type: Revenue
-- ============================================================

INSERT INTO erp.ChartOfAccounts (TenantId, Code, Name, Type, ParentCode, Level) VALUES
(0, '60',   N'Prihodi od prodaje',                                 'Revenue', NULL,  1),
(0, '600',  N'Prihodi od prodaje robe na domaćem tržištu',        'Revenue', '60',  2),
(0, '601',  N'Prihodi od prodaje robe na inostranom tržištu',     'Revenue', '60',  2),
(0, '602',  N'Prihodi od prodaje gotovih proizvoda',               'Revenue', '60',  2),
(0, '610',  N'Prihodi od usluga',                                  'Revenue', '60',  2),
(0, '611',  N'Prihodi od IT usluga',                               'Revenue', '60',  2),
(0, '612',  N'Prihodi od konsultantskih usluga',                   'Revenue', '60',  2),
(0, '62',   N'Ostali poslovni prihodi',                            'Revenue', NULL,  1),
(0, '620',  N'Prihodi od zakupa',                                  'Revenue', '62',  2),
(0, '621',  N'Subvencije, dotacije i ostali prihodi',              'Revenue', '62',  2),
(0, '66',   N'Finansijski prihodi',                                'Revenue', NULL,  1),
(0, '660',  N'Prihodi od kamata',                                  'Revenue', '66',  2),
(0, '661',  N'Pozitivne kursne razlike',                           'Revenue', '66',  2),
(0, '662',  N'Prihodi od dividendi i učešća u dobitku',            'Revenue', '66',  2),
(0, '67',   N'Ostali prihodi',                                     'Revenue', NULL,  1),
(0, '670',  N'Prihodi od prodaje stalne imovine',                  'Revenue', '67',  2),
(0, '671',  N'Prihodi po osnovu smanjenja obaveza',                'Revenue', '67',  2),
(0, '679',  N'Ostali nepomenuti prihodi',                          'Revenue', '67',  2);

COMMIT TRANSACTION;

PRINT N'Kontni plan RS uspešno unesen (' +
      CAST((SELECT COUNT(*) FROM erp.ChartOfAccounts WHERE TenantId = 0) AS NVARCHAR(10)) +
      N' naloga).';
GO
