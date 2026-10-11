# Report API

## Overview

The Report API generates aggregated analytics, breakdowns, and exports for reviewing expenses and overall cash flow.
It summarizes category-level spend across flat categories, tracks vendor spend via parties, and computes budget-versus-actual variances.

All amounts are calculated and returned in cents to ensure precision.

## Base URL

The base URL for all API endpoints is:
`https://[IP_ADDRESS]/api/v1`

## Supported Export Formats

Endpoints support a `format` query parameter:
- `json` (Default): Returns structured JSON payloads for web applications and dashboards.
- `csv`: Returns a downloadable comma-separated values file (`text/csv`) with `Content-Disposition: attachment`.
- `pdf`: Returns a formatted printable report document (`application/pdf`) with `Content-Disposition: attachment`.

## Endpoints

### Expense and Cash Flow Summary

Returns high-level totals (income, expenses, net cash flow) and period-by-period trend data.

**Endpoint:** `GET /report/expense-summary`

**Query Parameters:**

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `start_date` | timestamp | Yes | Start date for summary window (ISO 8601) |
| `end_date` | timestamp | Yes | End date for summary window (ISO 8601) |
| `currency` | string | No | Currency code to aggregate (default: `"EUR"`) |
| `group_by` | string | No | Trend grouping interval: `day`, `week`, `month`, `year` (default: `month`) |
| `format` | string | No | Output format: `json`, `csv`, `pdf` (default: `json`) |

**Response (200 OK - JSON format):**

```json
{
  "currency": "EUR",
  "start_date": "2023-01-01T00:00:00Z",
  "end_date": "2023-01-31T23:59:59Z",
  "total_expenses": 184550,
  "total_income": 350000,
  "net_cash_flow": 165450,
  "transaction_count": 42,
  "average_expense_amount": 4394,
  "trend": [
    {
      "period": "2023-01",
      "expenses": 184550,
      "income": 350000,
      "net": 165450,
      "transaction_count": 42
    }
  ]
}
```

---

### Category Spending Breakdown

Returns spending aggregated by each flat expense/income category, including each category's total amount, transaction count, and share of total spending.

**Endpoint:** `GET /report/by-category`

**Query Parameters:**

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `start_date` | timestamp | Yes | Start date for reporting window (ISO 8601) |
| `end_date` | timestamp | Yes | End date for reporting window (ISO 8601) |
| `currency` | string | No | Currency code (default: `"EUR"`) |
| `flow_type` | string | No | Filter by category flow type: `EXPENSE` or `INCOME` (default: `EXPENSE`) |
| `format` | string | No | Output format: `json`, `csv`, `pdf` (default: `json`) |

**Response (200 OK - JSON format):**

```json
{
  "currency": "EUR",
  "start_date": "2023-01-01T00:00:00Z",
  "end_date": "2023-01-31T23:59:59Z",
  "total_spend": 184550,
  "categories": [
    {
      "category_id": "123e4567-e89b-12d3-a456-426614174000",
      "category_name": "Groceries",
      "amount": 51200,
      "percentage_of_total": 27.74,
      "transaction_count": 14
    },
    {
      "category_id": "123e4567-e89b-12d3-a456-426614174001",
      "category_name": "Restaurants",
      "amount": 17250,
      "percentage_of_total": 9.35,
      "transaction_count": 4
    },
    {
      "category_id": "123e4567-e89b-12d3-a456-426614174003",
      "category_name": "Rent",
      "amount": 85000,
      "percentage_of_total": 46.06,
      "transaction_count": 1
    }
  ]
}
```

---

### Party / Vendor Spending Report

Ranks top counterparties and merchants by total spend amount.

**Endpoint:** `GET /report/by-party`

**Query Parameters:**

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `start_date` | timestamp | Yes | Start date (ISO 8601) |
| `end_date` | timestamp | Yes | End date (ISO 8601) |
| `currency` | string | No | Currency code (default: `"EUR"`) |
| `party_type` | string | No | Filter by party classification (e.g. `MERCHANT`, `INDIVIDUAL`) |
| `limit` | integer | No | Maximum number of top parties to return (default: `10`) |
| `format` | string | No | Output format: `json`, `csv` (default: `json`) |

**Response (200 OK - JSON format):**

```json
{
  "currency": "EUR",
  "start_date": "2023-01-01T00:00:00Z",
  "end_date": "2023-01-31T23:59:59Z",
  "total_party_spend": 172000,
  "parties": [
    {
      "party_id": "323e4567-e89b-12d3-a456-426614174000",
      "party_name": "Local Supermarket",
      "party_type": "MERCHANT",
      "total_amount": 51200,
      "percentage_of_spend": 29.77,
      "transaction_count": 14
    },
    {
      "party_id": "323e4567-e89b-12d3-a456-426614174010",
      "party_name": "City Utilities",
      "party_type": "ORGANIZATION",
      "total_amount": 23500,
      "percentage_of_spend": 13.66,
      "transaction_count": 2
    }
  ]
}
```

---

### Budget vs. Actual Variance Report

Compares budgeted targets from the Budget API against actual transactions from the Transaction API for a target period.

**Endpoint:** `GET /report/budget-variance`

**Query Parameters:**

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `period` | string | Yes | Period to review in `YYYY-MM` format (e.g. `"2023-01"`) |
| `currency` | string | No | Currency code (default: `"EUR"`) |
| `format` | string | No | Output format: `json`, `csv`, `pdf` (default: `json`) |

**Response (200 OK - JSON format):**

```json
{
  "period": "2023-01",
  "currency": "EUR",
  "total_budgeted": 150000,
  "total_actual": 142000,
  "total_variance": 8000,
  "overall_utilization_percentage": 94.67,
  "categories": [
    {
      "category_id": "123e4567-e89b-12d3-a456-426614174000",
      "category_name": "Groceries",
      "budget_amount": 50000,
      "actual_amount": 51200,
      "variance_amount": -1200,
      "utilization_percentage": 102.40,
      "is_over_budget": true
    },
    {
      "category_id": "123e4567-e89b-12d3-a456-426614174001",
      "category_name": "Restaurants",
      "budget_amount": 25000,
      "actual_amount": 17250,
      "variance_amount": 7750,
      "utilization_percentage": 69.00,
      "is_over_budget": false
    }
  ]
}
```

---

### Recurring vs. Variable Expense Report

Analyzes fixed/recurring expenditures (subscriptions, rent, utilities) compared against variable discretionary spending.

**Endpoint:** `GET /report/recurring-expenses`

**Query Parameters:**

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `start_date` | timestamp | Yes | Start date (ISO 8601) |
| `end_date` | timestamp | Yes | End date (ISO 8601) |
| `currency` | string | No | Currency code (default: `"EUR"`) |
| `format` | string | No | Output format: `json`, `csv` (default: `json`) |

**Response (200 OK - JSON format):**

```json
{
  "currency": "EUR",
  "start_date": "2023-01-01T00:00:00Z",
  "end_date": "2023-01-31T23:59:59Z",
  "total_expenses": 184550,
  "recurring": {
    "amount": 95000,
    "percentage": 51.48,
    "transaction_count": 6
  },
  "variable": {
    "amount": 89550,
    "percentage": 48.52,
    "transaction_count": 36
  }
}
```

---

### Export File Download Headers

When `format=csv` or `format=pdf` is provided in any reporting endpoint:

```http
HTTP/1.1 200 OK
Content-Type: text/csv; charset=utf-8
Content-Disposition: attachment; filename="expense-summary-2023-01.csv"
```
or
```http
HTTP/1.1 200 OK
Content-Type: application/pdf
Content-Disposition: attachment; filename="budget-variance-2023-01.pdf"
```
