# Expense-Type API

## Overview

The Expense-Type API manages a centralized, hierarchical list of expense categories used across the application.
Expense types have strict parent-child relationships, with a single level of hierarchy (e.g., "Food" → "Groceries").

All amounts are stored in cents to ensure precision.

## Base URL

The base URL for all API endpoints is:
`https://[IP_ADDRESS]/api/v1`

## Endpoints

### Create Expense Type

Creates a new expense type.

**Endpoint:** `POST /expense-type`

**Request Body:**

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `description` | string | Yes | Human-readable name of the expense type |
| `parent_id` | UUID | No | ID of the parent expense type (for hierarchical structure) |
| `is_active` | boolean | No | Whether the expense type is currently active |
| `type_code` | string | Yes | Unique system code (e.g., "FOOD_GROCERIES") |

**Response (201 Created):**

| Field | Type | Description |
|-------|------|-------------|
| `id` | UUID | Unique identifier |
| `description` | string | Human-readable name |
| `parent_id` | UUID | Parent ID (null for root) |
| `is_active` | boolean | Active status |
| `type_code` | string | System code |
| `created_at` | timestamp | Creation timestamp |
| `updated_at` | timestamp | Last update timestamp |

**Example:**

```http
POST /expense-type HTTP/1.1
Content-Type: application/json

{
  "description": "Food",
  "parent_id": null,
  "is_active": true,
  "type_code": "FOOD"
}
```

### List All Expense Types

Retrieves all expense types, optionally filtered by parent ID.

**Endpoint:** `GET /expense-type`

**Query Parameters:**

| Parameter | Type | Description |
|-----------|------|-------------|
| `parent_id` | UUID | Filter by parent ID |

**Response (200 OK):**

```json
[
  {
    "id": "123e4567-e89b-12d3-a456-426614174000",
    "description": "Food",
    "parent_id": null,
    "is_active": true,
    "type_code": "FOOD",
    "created_at": "2023-01-01T12:00:00Z",
    "updated_at": "2023-01-01T12:00:00Z"
  },
  {
    "id": "123e4567-e89b-12d3-a456-426614174001",
    "description": "Groceries",
    "parent_id": "123e4567-e89b-12d3-a456-426614174000",
    "is_active": true,
    "type_code": "FOOD_GROCERIES",
    "created_at": "2023-01-01T12:05:00Z",
    "updated_at": "2023-01-01T12:05:00Z"
  }
]
```

### Get Expense Type Details

Retrieves details of a single expense type by ID.

**Endpoint:** `GET /expense-type/{id}`

**Response (200 OK):**

| Field | Type | Description |
|-------|------|-------------|
| `id` | UUID | Unique identifier |
| `description` | string | Human-readable name |
| `parent_id` | UUID | Parent ID (null for root) |
| `is_active` | boolean | Active status |
| `type_code` | string | System code |
| `created_at` | timestamp | Creation timestamp |
| `updated_at` | timestamp | Last update timestamp |

### Update Expense Type

Updates an existing expense type.

**Endpoint:** `PUT /expense-type/{id}`

**Request Body:**

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `description` | string | No | New description |
| `parent_id` | UUID | No | New parent ID |
| `is_active` | boolean | No | New active status |
| `type_code` | string | No | New system code |

**Response (200 OK):**

Returns the updated expense type object.

### Delete Expense Type

Deletes an expense type by ID.

**Endpoint:** `DELETE /expense-type/{id}`

**Constraints:**
- An expense type cannot be deleted if it has any associated expenses.
- All associated expenses must be re-assigned to a different parent expense type before deletion.

**Response (204 No Content):**
- Success

**Error (400 Bad Request):**
- "Cannot delete expense type with associated expenses"
