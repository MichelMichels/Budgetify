# Category API

## Overview

The Category API manages a centralized, flat list of financial categories used across the application for both expenses and income (e.g., "Groceries", "Restaurants", "Rent", "Salary").
All categories exist in a single flat structure without hierarchies or subcategories.

All amounts are stored in cents to ensure precision.

## Base URL

The base URL for all API endpoints is:
`https://[IP_ADDRESS]/api/v1`

## Endpoints

### Create Category

Creates a new category.

**Endpoint:** `POST /category`

**Request Body:**

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `description` | string | Yes | Human-readable name of the category |
| `flow_type` | string | No | Cash-flow type (`EXPENSE` or `INCOME`, default: `EXPENSE`) |
| `is_active` | boolean | No | Whether the category is currently active (default: `true`) |
| `category_code` | string | Yes | Unique system code (e.g., "GROCERIES", "RENT") |

**Response (201 Created):**

| Field | Type | Description |
|-------|------|-------------|
| `id` | UUID | Unique identifier |
| `description` | string | Human-readable name |
| `flow_type` | string | Cash-flow classification (`EXPENSE` or `INCOME`) |
| `is_active` | boolean | Active status |
| `category_code` | string | System code |
| `created_at` | timestamp | Creation timestamp |
| `updated_at` | timestamp | Last update timestamp |

**Example:**

```http
POST /category HTTP/1.1
Content-Type: application/json

{
  "description": "Groceries",
  "flow_type": "EXPENSE",
  "is_active": true,
  "category_code": "GROCERIES"
}
```

### List All Categories

Retrieves all categories, optionally filtered by flow type or active status.

**Endpoint:** `GET /category`

**Query Parameters:**

| Parameter | Type | Description |
|-----------|------|-------------|
| `flow_type` | string | Filter by cash-flow type (`EXPENSE` or `INCOME`) |
| `is_active` | boolean | Filter by active status |

**Response (200 OK):**

```json
[
  {
    "id": "123e4567-e89b-12d3-a456-426614174000",
    "description": "Groceries",
    "flow_type": "EXPENSE",
    "is_active": true,
    "category_code": "GROCERIES",
    "created_at": "2023-01-01T12:00:00Z",
    "updated_at": "2023-01-01T12:00:00Z"
  },
  {
    "id": "123e4567-e89b-12d3-a456-426614174001",
    "description": "Restaurants",
    "flow_type": "EXPENSE",
    "is_active": true,
    "category_code": "RESTAURANTS",
    "created_at": "2023-01-01T12:05:00Z",
    "updated_at": "2023-01-01T12:05:00Z"
  },
  {
    "id": "123e4567-e89b-12d3-a456-426614174002",
    "description": "Salary",
    "flow_type": "INCOME",
    "is_active": true,
    "category_code": "INCOME_SALARY",
    "created_at": "2023-01-01T12:10:00Z",
    "updated_at": "2023-01-01T12:10:00Z"
  }
]
```

### Get Category Details

Retrieves details of a single category by ID.

**Endpoint:** `GET /category/{id}`

**Response (200 OK):**

| Field | Type | Description |
|-------|------|-------------|
| `id` | UUID | Unique identifier |
| `description` | string | Human-readable name |
| `flow_type` | string | Cash-flow classification (`EXPENSE` or `INCOME`) |
| `is_active` | boolean | Active status |
| `category_code` | string | System code |
| `created_at` | timestamp | Creation timestamp |
| `updated_at` | timestamp | Last update timestamp |

### Update Category

Updates an existing category.

**Endpoint:** `PUT /category/{id}`

**Request Body:**

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `description` | string | No | New description |
| `flow_type` | string | No | New cash-flow type (`EXPENSE` or `INCOME`) |
| `is_active` | boolean | No | New active status |
| `category_code` | string | No | New system code |

**Response (200 OK):**

Returns the updated category object.

### Delete Category

Deletes a category by ID.

**Endpoint:** `DELETE /category/{id}`

**Constraints:**
- A category cannot be deleted if it has any associated transactions.
- All associated transactions must be re-assigned to a different category before deletion.

**Response (204 No Content):**
- Success

**Error (400 Bad Request):**
- "Cannot delete category with associated transactions"
