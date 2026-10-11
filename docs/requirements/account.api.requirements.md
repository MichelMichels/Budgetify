# Account API

## Overview

The Account API manages user financial accounts (e.g., Checking, Savings, Credit Card, Cash, Investment).
It tracks real-time account balances and enables fund transfers between accounts without distorting operating expense or income reports.

All amounts are stored in cents to ensure precision.

## Base URL

The base URL for all API endpoints is:
`https://[IP_ADDRESS]/api/v1`

## Endpoints

### Create Account

Creates a new financial account.

**Endpoint:** `POST /account`

**Request Body:**

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `name` | string | Yes | Human-readable account name (e.g., "Main Checking", "Emergency Savings") |
| `account_type` | string | Yes | Account classification: `CHECKING`, `SAVINGS`, `CREDIT_CARD`, `CASH`, `INVESTMENT` |
| `currency` | string | No | 3-letter ISO 4217 currency code (default: `"EUR"`) |
| `initial_balance` | integer | No | Starting balance in cents (default: `0`, e.g. `250000` for 2,500.00) |
| `iban` | string | No | Optional International Bank Account Number |
| `is_active` | boolean | No | Whether the account is active (default: `true`) |
| `notes` | string | No | Optional notes or financial institution details |

**Response (201 Created):**

| Field | Type | Description |
|-------|------|-------------|
| `id` | UUID | Unique identifier |
| `name` | string | Account name |
| `account_type` | string | Account classification |
| `currency` | string | Currency code |
| `current_balance` | integer | Current account balance in cents |
| `iban` | string | Account IBAN (null if not provided) |
| `is_active` | boolean | Active status |
| `notes` | string | Additional details |
| `created_at` | timestamp | Creation timestamp |
| `updated_at` | timestamp | Last update timestamp |

**Example:**

```http
POST /account HTTP/1.1
Content-Type: application/json

{
  "name": "High-Yield Savings",
  "account_type": "SAVINGS",
  "currency": "EUR",
  "initial_balance": 1000000,
  "iban": "NL91ABNA0417164399",
  "is_active": true,
  "notes": "Emergency fund account"
}
```

### List All Accounts

Retrieves all financial accounts, optionally filtered by account type or active status.

**Endpoint:** `GET /account`

**Query Parameters:**

| Parameter | Type | Description |
|-----------|------|-------------|
| `account_type` | string | Filter by account type (`CHECKING`, `SAVINGS`, `CREDIT_CARD`, `CASH`, `INVESTMENT`) |
| `is_active` | boolean | Filter by active status |
| `currency` | string | Filter by currency code |

**Response (200 OK):**

```json
[
  {
    "id": "523e4567-e89b-12d3-a456-426614174000",
    "name": "Main Checking",
    "account_type": "CHECKING",
    "currency": "EUR",
    "current_balance": 184550,
    "iban": "NL91ABNA0417164300",
    "is_active": true,
    "notes": "Primary operating account",
    "created_at": "2023-01-01T00:00:00Z",
    "updated_at": "2023-01-15T14:30:00Z"
  },
  {
    "id": "523e4567-e89b-12d3-a456-426614174001",
    "name": "High-Yield Savings",
    "account_type": "SAVINGS",
    "currency": "EUR",
    "current_balance": 1050000,
    "iban": "NL91ABNA0417164399",
    "is_active": true,
    "notes": "Emergency fund account",
    "created_at": "2023-01-01T00:00:00Z",
    "updated_at": "2023-01-15T15:00:00Z"
  }
]
```

### Get Account Details

Retrieves details of a single account by ID, including its current balance.

**Endpoint:** `GET /account/{id}`

**Response (200 OK):**

| Field | Type | Description |
|-------|------|-------------|
| `id` | UUID | Unique identifier |
| `name` | string | Account name |
| `account_type` | string | Account classification |
| `currency` | string | Currency code |
| `current_balance` | integer | Current balance in cents |
| `iban` | string | Account IBAN |
| `is_active` | boolean | Active status |
| `notes` | string | Notes |
| `created_at` | timestamp | Creation timestamp |
| `updated_at` | timestamp | Last update timestamp |

### Update Account

Updates account metadata.

**Endpoint:** `PUT /account/{id}`

**Request Body:**

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `name` | string | No | New account name |
| `account_type` | string | No | New classification |
| `iban` | string | No | New IBAN |
| `is_active` | boolean | No | New active status |
| `notes` | string | No | New notes |

**Response (200 OK):**

Returns the updated account object.

### Delete Account

Deletes an account by ID.

**Endpoint:** `DELETE /account/{id}`

**Constraints:**
- An account cannot be deleted if it has any associated transactions or transfers.
- All associated transactions must be re-assigned or removed prior to deletion.

**Response (204 No Content):**
- Success

**Error (400 Bad Request):**
- "Cannot delete account with associated transactions"
