# Budget API

## Overview

The Budget API manages budget allocations and spending targets for categories across defined time periods (monthly or yearly).
Budgets provide the benchmark against which actual spending from the Transaction API is measured to compute variance, percentage utilized, and remaining allowance in reports.

All amounts are stored in cents to ensure precision.

## Base URL

The base URL for all API endpoints is:
`https://[IP_ADDRESS]/api/v1`

## Endpoints

### Create Budget

Creates a new budget target for a category and period.

**Endpoint:** `POST /budget`

**Request Body:**

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `category_id` | UUID | Yes | Category ID to apply the budget to (from Category API) |
| `period` | string | Yes | Budget period in `YYYY-MM` format (e.g., `"2023-01"`) |
| `amount` | integer | Yes | Budgeted target in cents (must be greater than 0, e.g. `50000` for 500.00) |
| `currency` | string | No | 3-letter ISO 4217 currency code (default: `"EUR"`) |
| `notes` | string | No | Optional budget notes or rationale |

**Response (201 Created):**

| Field | Type | Description |
|-------|------|-------------|
| `id` | UUID | Unique identifier |
| `category_id` | UUID | Associated category ID |
| `period` | string | Budget period (`YYYY-MM`) |
| `amount` | integer | Budgeted amount in cents |
| `currency` | string | Currency code |
| `notes` | string | Optional notes |
| `created_at` | timestamp | Creation timestamp |
| `updated_at` | timestamp | Last update timestamp |

**Example:**

```http
POST /budget HTTP/1.1
Content-Type: application/json

{
  "category_id": "123e4567-e89b-12d3-a456-426614174000",
  "period": "2023-01",
  "amount": 50000,
  "currency": "EUR",
  "notes": "Monthly groceries budget"
}
```

### List All Budgets

Retrieves budget targets, optionally filtered by period or category.

**Endpoint:** `GET /budget`

**Query Parameters:**

| Parameter | Type | Description |
|-----------|------|-------------|
| `period` | string | Filter by budget period (e.g. `"2023-01"`) |
| `category_id` | UUID | Filter by category ID |
| `currency` | string | Filter by currency code |

**Response (200 OK):**

```json
[
  {
    "id": "423e4567-e89b-12d3-a456-426614174000",
    "category_id": "123e4567-e89b-12d3-a456-426614174000",
    "period": "2023-01",
    "amount": 50000,
    "currency": "EUR",
    "notes": "Monthly groceries budget",
    "created_at": "2023-01-01T00:00:00Z",
    "updated_at": "2023-01-01T00:00:00Z"
  },
  {
    "id": "423e4567-e89b-12d3-a456-426614174001",
    "category_id": "123e4567-e89b-12d3-a456-426614174001",
    "period": "2023-01",
    "amount": 25000,
    "currency": "EUR",
    "notes": "Monthly Restaurants budget",
    "created_at": "2023-01-01T00:00:00Z",
    "updated_at": "2023-01-01T00:00:00Z"
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
| `period` | string | Budget period (`YYYY-MM`) |
| `amount` | integer | Budgeted amount in cents |
| `currency` | string | Currency code |
| `notes` | string | Notes |
| `created_at` | timestamp | Creation timestamp |
| `updated_at` | timestamp | Last update timestamp |

### Update Budget

Updates an existing budget.

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
- Does not affect past or current transactions in the category.

**Response (204 No Content):**
- Success

**Error (404 Not Found):**
- "Budget not found"
