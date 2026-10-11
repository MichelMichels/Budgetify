# Budget API

## Overview

The Budget API manages spending targets for categories across defined time periods.
It supports two budgeting modes:
1. **Recurring Monthly Budgets (`period: null`):** A perpetual monthly spending target that automatically applies to every month without needing to re-create entries (e.g., "Always budget €500/month for Groceries").
2. **Month-Specific Overrides (`period: "YYYY-MM"`):** An override target that applies exclusively to a specific calendar month (e.g., "Increase Food budget to €800 for December 2023").

Budgets provide the benchmark against which actual spending from the Transaction API is measured in variance reports.

All amounts are stored in cents to ensure precision.

## Base URL

The base URL for all API endpoints is:
`https://[IP_ADDRESS]/api/v1`

## Budgeting Rules & Uniqueness Constraints

1. **Resolution Priority in Reporting:**
   - When generating reports for a specific month (e.g., `2023-04`), the system resolves the category budget using:
     $$\text{Effective Budget} = \text{COALESCE}(\text{Specific Month Budget ("2023-04")}, \text{Recurring Budget (period is null)})$$
   - If neither exists, the category has no budget target for that month.
2. **Uniqueness Constraints:**
   - A category can have at most **one** recurring budget (`category_id` where `period` is `null`).
   - A category can have at most **one** budget per specific month `(category_id, period)`.
   - Creating a duplicate budget violates this constraint and returns `409 Conflict`.

## Endpoints

### Create Budget

Creates a new recurring budget or month-specific override.

**Endpoint:** `POST /budget`

**Request Body:**

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `category_id` | UUID | Yes | Category ID to apply the budget to (from Category API) |
| `period` | string | No | Budget period in `YYYY-MM` format (e.g., `"2023-01"`), or `null` / omitted for a recurring monthly baseline |
| `amount` | integer | Yes | Budgeted target in cents (must be greater than 0, e.g. `50000` for 500.00) |
| `currency` | string | No | 3-letter ISO 4217 currency code (default: `"EUR"`) |
| `notes` | string | No | Optional budget notes or rationale |

**Response (201 Created):**

| Field | Type | Description |
|-------|------|-------------|
| `id` | UUID | Unique identifier |
| `category_id` | UUID | Associated category ID |
| `period` | string | Budget period (`YYYY-MM`), or `null` for recurring monthly budget |
| `is_recurring` | boolean | `true` if `period` is null (recurring baseline); `false` if month-specific override |
| `amount` | integer | Budgeted amount in cents |
| `currency` | string | Currency code |
| `notes` | string | Optional notes |
| `created_at` | timestamp | Creation timestamp |
| `updated_at` | timestamp | Last update timestamp |

**Example 1: Perpetual Recurring Monthly Budget**

```http
POST /budget HTTP/1.1
Content-Type: application/json

{
  "category_id": "123e4567-e89b-12d3-a456-426614174000",
  "period": null,
  "amount": 50000,
  "currency": "EUR",
  "notes": "Default monthly groceries budget"
}
```

**Example 2: Month-Specific Override**

```http
POST /budget HTTP/1.1
Content-Type: application/json

{
  "category_id": "123e4567-e89b-12d3-a456-426614174000",
  "period": "2023-12",
  "amount": 80000,
  "currency": "EUR",
  "notes": "Higher budget for December holiday groceries"
}
```

### List All Budgets

Retrieves budget targets, optionally filtered by category, period, or recurrence.

**Endpoint:** `GET /budget`

**Query Parameters:**

| Parameter | Type | Description |
|-----------|------|-------------|
| `category_id` | UUID | Filter by category ID |
| `period` | string | Filter by budget period (e.g. `"2023-01"` or `"recurring"` for baselines with `period: null`) |
| `is_recurring` | boolean | Filter by recurring baselines (`true`) or month-specific overrides (`false`) |
| `currency` | string | Filter by currency code |

**Response (200 OK):**

```json
[
  {
    "id": "423e4567-e89b-12d3-a456-426614174000",
    "category_id": "123e4567-e89b-12d3-a456-426614174000",
    "period": null,
    "is_recurring": true,
    "amount": 50000,
    "currency": "EUR",
    "notes": "Default monthly groceries budget",
    "created_at": "2023-01-01T00:00:00Z",
    "updated_at": "2023-01-01T00:00:00Z"
  },
  {
    "id": "423e4567-e89b-12d3-a456-426614174001",
    "category_id": "123e4567-e89b-12d3-a456-426614174000",
    "period": "2023-12",
    "is_recurring": false,
    "amount": 80000,
    "currency": "EUR",
    "notes": "Higher budget for December holiday groceries",
    "created_at": "2023-11-15T10:00:00Z",
    "updated_at": "2023-11-15T10:00:00Z"
  }
]
```

### Get Budget Details

Retrieves details of a single budget target by ID.

**Endpoint:** `GET /budget/{id}`

**Response (200 OK):**

| Field | Type | Description |
|-------|------|-------------|
| `id` | UUID | Unique identifier |
| `category_id` | UUID | Associated category ID |
| `period` | string | Budget period (`YYYY-MM`) or `null` |
| `is_recurring` | boolean | Recurrence indicator |
| `amount` | integer | Budgeted amount in cents |
| `currency` | string | Currency code |
| `notes` | string | Notes |
| `created_at` | timestamp | Creation timestamp |
| `updated_at` | timestamp | Last update timestamp |

### Update Budget

Updates an existing budget target.

**Endpoint:** `PUT /budget/{id}`

**Request Body:**

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `amount` | integer | No | New budget amount in cents (must be greater than 0) |
| `notes` | string | No | New notes |

**Response (200 OK):**

Returns the updated budget object.

### Delete Budget

Deletes a budget target by ID.

**Endpoint:** `DELETE /budget/{id}`

**Constraints:**
- The budget target is permanently deleted.
- If a month-specific override is deleted, reports for that month automatically revert to the recurring baseline budget (if one exists).
- Does not affect transactions in the category.

**Response (204 No Content):**
- Success

**Error (404 Not Found):**
- "Budget not found"

**Error (409 Conflict):**
- "A budget for this category and period already exists"
