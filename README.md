# Budgetify

A personal finance and expense reporting platform built with .NET.

Budgetify allows users to track expenses and income across flat categories, link transactions to counterparties, manage multiple financial accounts with internal transfers, establish monthly recurring budgets, and generate analytical spending reports.

---

## Repository Structure

```text
Budgetify/
├── docs/
│   └── requirements/                # API and Domain Requirements
│       ├── account.api.requirements.md
│       ├── budget.api.requirements.md
│       ├── category.api.requirements.md
│       ├── party.api.requirements.md
│       ├── report.api.requirements.md
│       └── transaction.api.requirements.md
├── src/
│   ├── Budgetify.Api/               # ASP.NET Core Web API (Controllers, OpenAPI)
│   └── Budgetify.Core/              # Domain entities, validation rules, interfaces
├── tests/
│   └── Budgetify.UnitTests/         # xUnit unit and integration tests
├── .gitignore                       # Standard .NET gitignore
├── Budgetify.sln                    # Root solution file
└── README.md
```

---

## API Specifications

The system requirements and REST specifications are detailed under `docs/requirements/`:

- [Account API](docs/requirements/account.api.requirements.md): Manage checking, savings, credit cards, and track balances.
- [Category API](docs/requirements/category.api.requirements.md): Flat list of financial categories for expenses and income.
- [Party API](docs/requirements/party.api.requirements.md): Counterparty management (merchants, employers, vendors, IBANs).
- [Transaction API](docs/requirements/transaction.api.requirements.md): Record expenses, income, refunds (negative amounts), and internal transfers.
- [Budget API](docs/requirements/budget.api.requirements.md): Recurring monthly baseline targets and month-specific overrides.
- [Report API](docs/requirements/report.api.requirements.md): Aggregated summaries, category breakdowns, vendor rankings, budget variance, and CSV/PDF exports.

---

## Getting Started

### Prerequisites
- [.NET SDK](https://dotnet.microsoft.com/) (.NET 10 / .NET 9)

### Build
```bash
dotnet build
```

### Run the API
```bash
dotnet run --project src/Budgetify.Api
```

### Run Tests
```bash
dotnet test
```
