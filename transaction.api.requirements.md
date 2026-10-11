# Transaction API

## Overview

The Transaction API manages financial records (expenses and income) across the application.
Transactions are associated with categories defined in the Category API and counterparties defined in the Party API to enable budgeting, reporting, and cash-flow tracking.

All amounts are stored in cents to ensure precision.

## Base URL

The base URL for all API endpoints is:
`https://[IP_ADDRESS]/api/v1`

## Endpoints

### Create Transaction

Creates a new transaction record.

**Endpoint:** `POST /transaction`

**Request Body:**

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `amount` | integer | Yes | Transaction amount in cents (e.g., `4599` for 45.99) |
| `currency` | string | No | 3-letter ISO 4217 currency code (e.g., `"EUR"`, `"USD"`, default: `"EUR"`) |
| `type` | string | Yes | Transaction type (`EXPENSE` or `INCOME`) |
| `category_id` | UUID | Yes | ID of the associated category (from Category API) |
| `party_id` | UUID | No | ID of the associated party / counterparty (from Party API) |
| `payment_method` | string | No | Payment method (`CREDIT_CARD`, `DEBIT_CARD`, `BANK_TRANSFER`, `CASH`, `OTHER`) |
| `is_recurring` | boolean | No | Whether the transaction is recurring (e.g. subscription, rent) (default: `false`) |
| `transaction_date` | timestamp | Yes | Date and time the transaction took place (ISO 8601) |
| `description` | string | Yes | Human-readable title or memo |
| `notes` | string | No | Optional additional details or notes |

**Response (201 Created):**

| Field | Type | Description |
|-------|------|-------------|
| `id` | UUID | Unique identifier |
| `amount` | integer | Amount in cents |
| `currency` | string | ISO 4217 currency code |
| `type` | string | Transaction type (`EXPENSE` or `INCOME`) |
| `category_id` | UUID | Associated category ID |
| `party_id` | UUID | Associated party ID |
| `payment_method` | string | Payment method |
| `is_recurring` | boolean | Recurring payment flag |
| `transaction_date` | timestamp | Transaction timestamp |
| `description` | string | Human-readable title |
| `notes` | string | Additional details |
| `created_at` | timestamp | Creation timestamp |
| `updated_at` | timestamp | Last update timestamp |

**Example:**

```http
POST /transaction HTTP/1.1
Content-Type: application/json

{
  "amount": 4599,
  "currency": "EUR",
  "type": "EXPENSE",
  "category_id": "123e4567-e89b-12d3-a456-426614174000",
  "party_id": "323e4567-e89b-12d3-a456-426614174000",
  "payment_method": "DEBIT_CARD",
  "is_recurring": false,
  "transaction_date": "2023-01-15T14:30:00Z",
  "description": "Weekly groceries",
  "notes": "Purchased organic produce and dairy"
}
```

### List All Transactions

Retrieves transactions, optionally filtered by date range, category, party, payment method, recurring status, or transaction type.

**Endpoint:** `GET /transaction`

**Query Parameters:**

| Parameter | Type | Description |
|-----------|------|-------------|
| `start_date` | timestamp | Filter transactions on or after this date (ISO 8601) |
| `end_date` | timestamp | Filter transactions on or before this date (ISO 8601) |
| `category_id` | UUID | Filter by associated category ID |
| `party_id` | UUID | Filter by associated party ID |
| `type` | string | Filter by transaction type (`EXPENSE` or `INCOME`) |
| `currency` | string | Filter by currency code |
| `payment_method` | string | Filter by payment method |
| `is_recurring` | boolean | Filter by recurring status |
| `limit` | integer | Maximum number of transactions to return (default: 50) |
| `offset` | integer | Number of records to skip for pagination (default: 0) |

**Response (200 OK):**

```json
[
  {
    "id": "223e4567-e89b-12d3-a456-426614174000",
    "amount": 4599,
    "currency": "EUR",
    "type": "EXPENSE",
    "category_id": "123e4567-e89b-12d3-a456-426614174000",
    "party_id": "323e4567-e89b-12d3-a456-426614174000",
    "payment_method": "DEBIT_CARD",
    "is_recurring": false,
    "transaction_date": "2023-01-15T14:30:00Z",
    "description": "Weekly groceries",
    "notes": "Purchased organic produce and dairy",
    "created_at": "2023-01-15T14:32:00Z",
    "updated_at": "2023-01-15T14:32:00Z"
  },
  {
    "id": "223e4567-e89b-12d3-a456-426614174001",
    "amount": 350000,
    "currency": "EUR",
    "type": "INCOME",
    "category_id": "123e4567-e89b-12d3-a456-426614174002",
    "party_id": "323e4567-e89b-12d3-a456-426614174001",
    "payment_method": "BANK_TRANSFER",
    "is_recurring": true,
    "transaction_date": "2023-01-01T09:00:00Z",
    "description": "Monthly Salary",
    "notes": "January salary deposit",
    "created_at": "2023-01-01T09:05:00Z",
    "updated_at": "2023-01-01T09:05:00Z"
  }
]
```

### Get Transaction Details

Retrieves details of a single transaction by ID.

**Endpoint:** `GET /transaction/{id}`

**Response (200 OK):**

| Field | Type | Description |
|-------|------|-------------|
| `id` | UUID | Unique identifier |
| `amount` | integer | Amount in cents |
| `currency` | string | ISO 4217 currency code |
| `type` | string | Transaction type (`EXPENSE` or `INCOME`) |
| `category_id` | UUID | Associated category ID |
| `party_id` | UUID | Associated party ID |
| `payment_method` | string | Payment method |
| `is_recurring` | boolean | Recurring payment flag |
| `transaction_date` | timestamp | Transaction timestamp |
| `description` | string | Human-readable title |
| `notes` | string | Additional details |
| `created_at` | timestamp | Creation timestamp |
| `updated_at` | timestamp | Last update timestamp |

### Update Transaction

Updates an existing transaction.

**Endpoint:** `PUT /transaction/{id}`

**Request Body:**

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `amount` | integer | No | New amount in cents (must be greater than 0) |
| `currency` | string | No | New ISO 4217 currency code |
| `type` | string | No | New transaction type (`EXPENSE` or `INCOME`) |
| `category_id` | UUID | No | New associated category ID |
| `party_id` | UUID | No | New associated party ID |
| `payment_method` | string | No | New payment method |
| `is_recurring` | boolean | No | New recurring status |
| `transaction_date` | timestamp | No | New transaction timestamp (ISO 8601) |
| `description` | string | No | New description |
| `notes` | string | No | New notes |

**Response (200 OK):**

Returns the updated transaction object.

### Delete Transaction

Deletes a transaction by ID.

**Endpoint:** `DELETE /transaction/{id}`

**Constraints:**
- The transaction record is permanently deleted.
- Does not affect the associated category or party.

**Response (204 No Content):**
- Success

**Error (404 Not Found):**
- "Transaction not found"
