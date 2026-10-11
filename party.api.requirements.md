# Party API

## Overview

The Party API manages external entities and counterparties involved in financial transactions, such as merchants, vendors, employers, service providers, and individuals.
Parties are linked to transactions to facilitate counterparty tracking, vendor reporting, and cash-flow categorization.

## Base URL

The base URL for all API endpoints is:
`https://[IP_ADDRESS]/api/v1`

## Lifecycle Rules

### Soft Deactivation vs. Hard Deletion
- **Soft Deactivation (`is_active: false`):**
  - When a merchant or counterparty is no longer in use, update `is_active: false` via `PUT /party/{id}`.
  - Deactivated parties are excluded from active selection lists when creating or updating transactions.
  - Deactivated parties **remain fully intact** on past transactions and historical vendor reporting.
- **Hard Deletion (`DELETE /party/{id}`):**
  - A party can **only** be permanently deleted if it has **zero associated transactions**.
  - If any transactions are linked to the party, deletion is blocked (`400 Bad Request`).

## Endpoints

### Create Party

Creates a new party.

**Endpoint:** `POST /party`

**Request Body:**

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `name` | string | Yes | Human-readable name of the merchant, vendor, or entity |
| `party_type` | string | Yes | Classification of the party (`MERCHANT`, `EMPLOYER`, `INDIVIDUAL`, `ORGANIZATION`) |
| `is_active` | boolean | No | Whether the party is currently active (default: `true`) |
| `party_code` | string | Yes | Unique system code (e.g., "MERCHANT_SUPERMARKET_01") |
| `iban` | string | No | Optional International Bank Account Number (e.g., "NL91ABNA0417164300") |
| `notes` | string | No | Optional additional details or notes |

**Response (201 Created):**

| Field | Type | Description |
|-------|------|-------------|
| `id` | UUID | Unique identifier |
| `name` | string | Name of the party |
| `party_type` | string | Classification of the party |
| `is_active` | boolean | Active status |
| `party_code` | string | System code |
| `iban` | string | International Bank Account Number (null if not provided) |
| `notes` | string | Additional details |
| `created_at` | timestamp | Creation timestamp |
| `updated_at` | timestamp | Last update timestamp |

**Example:**

```http
POST /party HTTP/1.1
Content-Type: application/json

{
  "name": "Local Supermarket",
  "party_type": "MERCHANT",
  "is_active": true,
  "party_code": "MERCHANT_LOCAL_SUPERMARKET",
  "iban": "NL91ABNA0417164300",
  "notes": "Main grocery store downtown"
}
```

### List All Parties

Retrieves all parties, optionally filtered by party type or active status.

**Endpoint:** `GET /party`

**Query Parameters:**

| Parameter | Type | Description |
|-----------|------|-------------|
| `party_type` | string | Filter by party classification (`MERCHANT`, `EMPLOYER`, `INDIVIDUAL`, `ORGANIZATION`) |
| `is_active` | boolean | Filter by active status (pass `true` to list only active parties for forms/pickers) |
| `search` | string | Case-insensitive search on name or party code |

**Response (200 OK):**

```json
[
  {
    "id": "323e4567-e89b-12d3-a456-426614174000",
    "name": "Local Supermarket",
    "party_type": "MERCHANT",
    "is_active": true,
    "party_code": "MERCHANT_LOCAL_SUPERMARKET",
    "iban": "NL91ABNA0417164300",
    "notes": "Main grocery store downtown",
    "created_at": "2023-01-01T10:00:00Z",
    "updated_at": "2023-01-01T10:00:00Z"
  },
  {
    "id": "323e4567-e89b-12d3-a456-426614174001",
    "name": "Employer Inc.",
    "party_type": "EMPLOYER",
    "is_active": true,
    "party_code": "EMPLOYER_INC",
    "iban": null,
    "notes": "Primary employment salary source",
    "created_at": "2023-01-01T10:05:00Z",
    "updated_at": "2023-01-01T10:05:00Z"
  }
]
```

### Get Party Details

Retrieves details of a single party by ID.

**Endpoint:** `GET /party/{id}`

**Response (200 OK):**

| Field | Type | Description |
|-------|------|-------------|
| `id` | UUID | Unique identifier |
| `name` | string | Name of the party |
| `party_type` | string | Classification of the party |
| `is_active` | boolean | Active status |
| `party_code` | string | System code |
| `iban` | string | International Bank Account Number |
| `notes` | string | Additional details |
| `created_at` | timestamp | Creation timestamp |
| `updated_at` | timestamp | Last update timestamp |

### Update Party

Updates an existing party. Use this endpoint to soft-deactivate a party by setting `is_active: false`.

**Endpoint:** `PUT /party/{id}`

**Request Body:**

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `name` | string | No | New name |
| `party_type` | string | No | New classification |
| `is_active` | boolean | No | New active status (`false` to deactivate) |
| `party_code` | string | No | New system code |
| `iban` | string | No | New IBAN |
| `notes` | string | No | New notes |

**Response (200 OK):**

Returns the updated party object.

### Delete Party

Deletes a party by ID.

**Endpoint:** `DELETE /party/{id}`

**Constraints:**
- A party cannot be deleted if it has any associated transactions.
- All associated transactions must be re-assigned to a different party or removed before deletion.
- For counterparties with historical transactions, use soft deactivation (`is_active: false`) instead.

**Response (204 No Content):**
- Success

**Error (400 Bad Request):**
- "Cannot delete party with associated transactions. Use soft deactivation instead."
