# Transaction API

## Overview

The Transaction API manages financial records across the application: expenses, income, and transfers.
Transactions are associated with:
- An origin account defined in the Account API (`account_id`).
- For transfers: a destination account (`destination_account_id`) to move money between accounts (e.g., Checking → Savings) without distorting expense reports.
- Categories defined in the Category API (`category_id`) for expense and income classification.
- External counterparties defined in the Party API (`party_id`).

All amounts are stored in cents to ensure precision.

## Base URL

The base URL for all API endpoints is:
`https://[IP_ADDRESS]/api/v1`

## Transaction Types and Balance Rules

| Type | Description | Source Account (`account_id`) | Destination Account (`destination_account_id`) | Category Requirement |
|------|-------------|-------------------------------|------------------------------------------------|----------------------|
| `EXPENSE` | Money spent at an external merchant (or refund received) | Decreases balance (`-amount`) | N/A (`null`) | Required |
| `INCOME` | Money received from an employer/source | Increases balance (`+amount`) | N/A (`null`) | Required |
| `TRANSFER` | Money moved between internal accounts | Decreases balance (`-amount`) | Increases balance (`+amount`) | Optional |

### Negative Amounts & Refunds
- **Negative Expenses (Refunds/Returns):** An `EXPENSE` transaction may have a negative amount (e.g., `-3000` for a €30.00 merchant refund). A negative expense reduces total reported category spend and increases the source account balance by €30.00.
- **Negative Income:** An `INCOME` transaction with a negative amount reflects an income clawback or deduction.
- **Zero Amounts:** An amount of `0` is invalid.

## Validation Rules

1. **Category Flow Type Matching:**
   - When `type` is `EXPENSE`, the assigned `category_id` must have `flow_type: "EXPENSE"`.
   - When `type` is `INCOME`, the assigned `category_id` must have `flow_type: "INCOME"`.
   - Creating or updating a transaction with mismatched category flow types must return `400 Bad Request`.
2. **Transfer Validation:**
   - When `type` is `TRANSFER`, `destination_account_id` is required and must not equal `account_id`.
   - `amount` for transfers must be a positive integer.

## Endpoints

### Create Transaction

Creates a new transaction record.

**Endpoint:** `POST /transaction`

**Request Body:**

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `amount` | integer | Yes | Transaction amount in cents (positive for standard spend/income, negative for refunds/returns; cannot be 0) |
| `currency` | string | No | 3-letter ISO 4217 currency code (default: `"EUR"`) |
| `type` | string | Yes | Transaction type (`EXPENSE`, `INCOME`, or `TRANSFER`) |
| `account_id` | UUID | Yes | Source account ID (or account receiving income) |
| `destination_account_id` | UUID | Conditional | Destination account ID (required if `type` is `TRANSFER`) |
| `category_id` | UUID | Conditional | Category ID (required for `EXPENSE` and `INCOME`, optional for `TRANSFER`) |
| `party_id` | UUID | No | Associated external party ID (merchant or employer) |
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
| `type` | string | Transaction type (`EXPENSE`, `INCOME`, `TRANSFER`) |
| `account_id` | UUID | Source account ID |
| `destination_account_id` | UUID | Destination account ID (null if not a transfer) |
| `category_id` | UUID | Associated category ID |
| `party_id` | UUID | Associated party ID |
| `payment_method` | string | Payment method |
| `is_recurring` | boolean | Recurring payment flag |
| `transaction_date` | timestamp | Transaction timestamp |
| `description` | string | Human-readable title |
| `notes` | string | Additional details |
| `created_at` | timestamp | Creation timestamp |
| `updated_at` | timestamp | Last update timestamp |

**Example 1: Expense**

```http
POST /transaction HTTP/1.1
Content-Type: application/json

{
  "amount": 4599,
  "currency": "EUR",
  "type": "EXPENSE",
  "account_id": "523e4567-e89b-12d3-a456-426614174000",
  "destination_account_id": null,
  "category_id": "123e4567-e89b-12d3-a456-426614174000",
  "party_id": "323e4567-e89b-12d3-a456-426614174000",
  "payment_method": "DEBIT_CARD",
  "is_recurring": false,
  "transaction_date": "2023-01-15T14:30:00Z",
  "description": "Weekly groceries",
  "notes": "Purchased organic produce and dairy"
}
```

**Example 2: Refund (Negative Expense)**

```http
POST /transaction HTTP/1.1
Content-Type: application/json

{
  "amount": -2500,
  "currency": "EUR",
  "type": "EXPENSE",
  "account_id": "523e4567-e89b-12d3-a456-426614174000",
  "destination_account_id": null,
  "category_id": "123e4567-e89b-12d3-a456-426614174000",
  "party_id": "323e4567-e89b-12d3-a456-426614174000",
  "payment_method": "DEBIT_CARD",
  "is_recurring": false,
  "transaction_date": "2023-01-18T10:00:00Z",
  "description": "Returned grocery item",
  "notes": "Refund credited to debit card"
}
```

**Example 3: Savings Transfer**

```http
POST /transaction HTTP/1.1
Content-Type: application/json

{
  "amount": 50000,
  "currency": "EUR",
  "type": "TRANSFER",
  "account_id": "523e4567-e89b-12d3-a456-426614174000",
  "destination_account_id": "523e4567-e89b-12d3-a456-426614174001",
  "category_id": null,
  "party_id": null,
  "payment_method": "BANK_TRANSFER",
  "is_recurring": true,
  "transaction_date": "2023-01-15T15:00:00Z",
  "description": "Monthly transfer to Savings",
  "notes": "Emergency fund savings goal"
}
```

### List All Transactions

Retrieves transactions, optionally filtered by date range, account, category, party, payment method, recurring status, or transaction type.

**Endpoint:** `GET /transaction`

**Query Parameters:**

| Parameter | Type | Description |
|-----------|------|-------------|
| `start_date` | timestamp | Filter transactions on or after this date (ISO 8601) |
| `end_date` | timestamp | Filter transactions on or before this date (ISO 8601) |
| `account_id` | UUID | Filter by source account ID |
| `destination_account_id` | UUID | Filter by destination account ID |
| `category_id` | UUID | Filter by associated category ID |
| `party_id` | UUID | Filter by associated party ID |
| `type` | string | Filter by transaction type (`EXPENSE`, `INCOME`, `TRANSFER`) |
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
    "account_id": "523e4567-e89b-12d3-a456-426614174000",
    "destination_account_id": null,
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
    "id": "223e4567-e89b-12d3-a456-426614174002",
    "amount": 50000,
    "currency": "EUR",
    "type": "TRANSFER",
    "account_id": "523e4567-e89b-12d3-a456-426614174000",
    "destination_account_id": "523e4567-e89b-12d3-a456-426614174001",
    "category_id": null,
    "party_id": null,
    "payment_method": "BANK_TRANSFER",
    "is_recurring": true,
    "transaction_date": "2023-01-15T15:00:00Z",
    "description": "Monthly transfer to Savings",
    "notes": "Emergency fund savings goal",
    "created_at": "2023-01-15T15:00:00Z",
    "updated_at": "2023-01-15T15:00:00Z"
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
| `type` | string | Transaction type (`EXPENSE`, `INCOME`, `TRANSFER`) |
| `account_id` | UUID | Source account ID |
| `destination_account_id` | UUID | Destination account ID |
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
| `amount` | integer | No | New amount in cents (cannot be 0) |
| `currency` | string | No | New ISO 4217 currency code |
| `type` | string | No | New transaction type (`EXPENSE`, `INCOME`, `TRANSFER`) |
| `account_id` | UUID | No | New source account ID |
| `destination_account_id` | UUID | No | New destination account ID |
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
- Reverses balance changes applied to `account_id` (and `destination_account_id` if a transfer).
- Does not affect the associated category, account, or party entities.

**Response (204 No Content):**
- Success

**Error (404 Not Found):**
- "Transaction not found"
