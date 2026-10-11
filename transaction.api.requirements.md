# Transaction API

## Overview

The Transaction API manages financial records (expenses and income) across the application.
Transactions are associated with expense types defined in the Expense-Type API and parties defined in the Party API to enable category-based budgeting, counterparty tracking, and reporting.

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
| `type` | string | Yes | Transaction type (`EXPENSE` or `INCOME`) |
| `expense_type_id` | UUID | Conditional | ID of the associated expense type (required if `type` is `EXPENSE`) |
| `party_id` | UUID | No | ID of the associated party / counterparty |
| `transaction_date` | timestamp | Yes | Date and time the transaction took place (ISO 8601) |
| `description` | string | Yes | Human-readable title or memo |
| `notes` | string | No | Optional additional details or notes |

**Response (201 Created):**

| Field | Type | Description |
|-------|------|-------------|
| `id` | UUID | Unique identifier |
| `amount` | integer | Amount in cents |
| `type` | string | Transaction type (`EXPENSE` or `INCOME`) |
| `expense_type_id` | UUID | Associated expense type ID |
| `party_id` | UUID | Associated party ID |
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
  "type": "EXPENSE",
  "expense_type_id": "123e4567-e89b-12d3-a456-426614174001",
  "party_id": "323e4567-e89b-12d3-a456-426614174000",
  "transaction_date": "2023-01-15T14:30:00Z",
  "description": "Weekly groceries",
  "notes": "Purchased organic produce and dairy"
}
```

### List All Transactions

Retrieves transactions, optionally filtered by date range, expense type, party, or transaction type.

**Endpoint:** `GET /transaction`

**Query Parameters:**

| Parameter | Type | Description |
|-----------|------|-------------|
| `start_date` | timestamp | Filter transactions on or after this date (ISO 8601) |
| `end_date` | timestamp | Filter transactions on or before this date (ISO 8601) |
| `expense_type_id` | UUID | Filter by associated expense type ID |
| `party_id` | UUID | Filter by associated party ID |
| `type` | string | Filter by transaction type (`EXPENSE` or `INCOME`) |
| `limit` | integer | Maximum number of transactions to return (default: 50) |
| `offset` | integer | Number of records to skip for pagination (default: 0) |

**Response (200 OK):**

```json
[
  {
    "id": "223e4567-e89b-12d3-a456-426614174000",
    "amount": 4599,
    "type": "EXPENSE",
    "expense_type_id": "123e4567-e89b-12d3-a456-426614174001",
    "party_id": "323e4567-e89b-12d3-a456-426614174000",
    "transaction_date": "2023-01-15T14:30:00Z",
    "description": "Weekly groceries",
    "notes": "Purchased organic produce and dairy",
    "created_at": "2023-01-15T14:32:00Z",
    "updated_at": "2023-01-15T14:32:00Z"
  },
  {
    "id": "223e4567-e89b-12d3-a456-426614174001",
    "amount": 350000,
    "type": "INCOME",
    "expense_type_id": null,
    "party_id": "323e4567-e89b-12d3-a456-426614174001",
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
| `type` | string | Transaction type (`EXPENSE` or `INCOME`) |
| `expense_type_id` | UUID | Associated expense type ID |
| `party_id` | UUID | Associated party ID |
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
| `type` | string | No | New transaction type (`EXPENSE` or `INCOME`) |
| `expense_type_id` | UUID | No | New associated expense type ID |
| `party_id` | UUID | No | New associated party ID |
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
- Does not affect the associated expense type or party.

**Response (204 No Content):**
- Success

**Error (404 Not Found):**
- "Transaction not found"
