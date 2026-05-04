# Database Setup

## Option A — EF Core Migrations (recommended)
```bash
cd src/AccountingERP.Infrastructure
dotnet ef migrations add InitialCreate --startup-project ../AccountingERP.API
dotnet ef database update --startup-project ../AccountingERP.API
```

## Option B — Manual SQL scripts
Run scripts in order:
1. `001_schema.sql`
2. `002_seed_chart_of_accounts.sql`  
3. `003_seed_demo_tenant.sql`
