# REST API Specification

This document provides complete reference documentation for the **Wp1Fall26Aws** HTTP REST API.

- **Base URL (Local Development):** `http://localhost:5180`
- **Protocols:** HTTP/1.1, HTTP/2
- **Data Exchange Format:** JSON (`application/json`) & Multipart Form Data (`multipart/form-data`)
- **CORS Support:** Enabled across all endpoints (`AllowAnyOrigin`, `AllowAnyHeader`, `AllowAnyMethod`)

---

## Endpoint Summary Matrix

| Method | Endpoint | Description | Request Format | Status Codes |
| :--- | :--- | :--- | :--- | :--- |
| `GET` | `/health` | Health and liveness probe | *None* | `200 OK` |
| `GET` | `/documents` | Retrieve all document records | *None* | `200 OK` |
| `GET` | `/documents/{id}` | Retrieve document by ID or S3 ObjectKey | *Path parameter* | `200 OK`, `404 Not Found` |
| `POST` | `/documents` | Upload a file or create text document | `multipart/form-data` or `application/json` | `201 Created`, `400 Bad Request`, `500 Server Error` |
| `PUT` | `/documents/{id}` | Update document metadata | `application/json` | `200 OK`, `400 Bad Request`, `404 Not Found` |
| `DELETE` | `/documents/{id}` | Delete document record by ID | *Path parameter* | `200 OK`, `404 Not Found` |

---

## 1. Health Probe (`GET /health`)

Verifies service availability. Used by client apps, AWS Elastic Beanstalk health checkers, and deployment verification scripts.

### Request
```http
GET /health HTTP/1.1
Host: localhost:5180
Accept: application/json
```

### Response
- **Status:** `200 OK`
- **Content-Type:** `application/json`

```json
{
  "status": "Healthy"
}
```

### cURL Example
```bash
curl -i http://localhost:5180/health
```

---

## 2. Retrieve All Documents (`GET /documents`)

Returns a list of all stored document records, sorted by upload time in descending order (newest first).

### Request
```http
GET /documents HTTP/1.1
Host: localhost:5180
Accept: application/json
```

### Response
- **Status:** `200 OK`
- **Content-Type:** `application/json`

```json
[
  {
    "id": "doc-2",
    "objectKey": "documents/uploads/2026/10/12/sample-readme.txt",
    "fileName": "readme.txt",
    "contentType": "text/plain",
    "contentLength": 2048,
    "uploadedUtc": "2026-10-16T14:55:00Z",
    "title": "Release Notes & Guide",
    "description": "Deployment guide and endpoint documentation for consumer app.",
    "updatedUtc": null,
    "metadata": {
      "original-filename": "readme.txt"
    }
  },
  {
    "id": "doc-1",
    "objectKey": "documents/uploads/2026/10/12/sample-project-spec.pdf",
    "fileName": "project-spec.pdf",
    "contentType": "application/pdf",
    "contentLength": 104857,
    "uploadedUtc": "2026-10-16T13:40:00Z",
    "title": "Project Specification",
    "description": "Architecture and design documentation for cloud storage service.",
    "updatedUtc": null,
    "metadata": {
      "original-filename": "project-spec.pdf"
    }
  }
]
```

### cURL Example
```bash
curl -i http://localhost:5180/documents
```

---

## 3. Retrieve Single Document (`GET /documents/{id}`)

Retrieves detailed metadata for a single document matching the given repository `id` or S3 `objectKey`.

### Parameters
| Name | In | Type | Required | Description |
| :--- | :--- | :--- | :--- | :--- |
| `id` | path | string | Yes | The document unique ID (e.g. `doc-1` or 32-char GUID) or full S3 object key |

### Responses
#### Success (`200 OK`)
```json
{
  "id": "doc-1",
  "objectKey": "documents/uploads/2026/10/12/sample-project-spec.pdf",
  "fileName": "project-spec.pdf",
  "contentType": "application/pdf",
  "contentLength": 104857,
  "uploadedUtc": "2026-10-16T13:40:00Z",
  "title": "Project Specification",
  "description": "Architecture and design documentation for cloud storage service.",
  "updatedUtc": null,
  "metadata": {
    "original-filename": "project-spec.pdf"
  }
}
```

#### Not Found (`404 Not Found`)
```json
{
  "error": "Document with ID 'non-existent-id' was not found."
}
```

### cURL Example
```bash
curl -i http://localhost:5180/documents/doc-1
```

---

## 4. Create / Upload Document (`POST /documents`)

The API accepts two mutually exclusive request content types:
1. `multipart/form-data`: For binary or text file uploads streamed into S3.
2. `application/json`: For quick text documents constructed directly from JSON properties.

### Validation Policy (Both Modes)
- **Maximum File Size:** 1,048,576 bytes (1 MiB)
- **Minimum File Size:** 1 byte (empty files rejected)
- **Allowed Extensions:** `.txt`, `.pdf`, `.png`, `.jpg`, `.jpeg`
- **Allowed Content Types:** `text/plain`, `application/pdf`, `image/png`, `image/jpeg`

---

### Mode A: Multipart Form Data Upload

#### Request Headers
- `Content-Type: multipart/form-data; boundary=----WebKitFormBoundary...`

#### Form Fields
| Field Name | Type | Required | Description |
| :--- | :--- | :--- | :--- |
| `file` | Binary | Yes | The file payload to upload |
| `title` | Text | No | Document title. If omitted, defaults to filename without extension |
| `description` | Text | No | Optional description text |

#### Success Response (`201 Created`)
- **Headers:** `Location: /documents/{objectKey}`

```json
{
  "id": "e4a3b8cd102f43899f8ab1009872cdef",
  "objectKey": "documents/uploads/2026/10/16/e2b4f91d08ac433ab9938b29df910041.txt",
  "metadata": {
    "original-filename": "report.txt",
    "content-type": "text/plain",
    "uploaded-utc": "2026-10-16T20:30:00.1234567Z",
    "content-length": "42"
  },
  "title": "Quarterly Report",
  "description": "Uploaded via multipart form",
  "fileName": "report.txt",
  "contentType": "text/plain",
  "contentLength": 42,
  "uploadedUtc": "2026-10-16T20:30:00.1234567Z"
}
```

#### Failure Response (`400 Bad Request`)
```json
{
  "errors": [
    "Extension '.exe' is not allowed.",
    "Content type 'application/x-msdownload' is not allowed."
  ]
}
```

#### cURL Example (File Upload)
```bash
curl -i -X POST http://localhost:5180/documents \
  -F "title=Sample Document" \
  -F "description=Uploaded via curl" \
  -F "file=@samples/synthetic-document.txt;type=text/plain"
```

---

### Mode B: JSON Payload Document Creation

#### Request Headers
- `Content-Type: application/json`

#### Request Body Schema
| Property | Type | Required | Description |
| :--- | :--- | :--- | :--- |
| `title` | string | **Yes** | Document title |
| `description` | string | No | Document description |
| `fileName` | string | No | Filename (e.g. `note.txt`). Defaults to `{guid}.txt` |
| `contentType` | string | No | MIME type. Defaults to `text/plain` |
| `content` | string | No | String content written into the file stream |

#### Request Example
```json
{
  "title": "Architecture Overview Note",
  "description": "Summary of system architecture and AWS S3 integration.",
  "fileName": "architecture-note.txt",
  "contentType": "text/plain",
  "content": "This document contains notes on the ASP.NET Core and S3 integration."
}
```

#### Success Response (`201 Created`)
- **Headers:** `Location: /documents/{objectKey}`

```json
{
  "id": "8f7e6d5c4b3a2109876543210fedcba9",
  "objectKey": "documents/uploads/2026/10/16/a1b2c3d4e5f67890123456789abcdef0.txt",
  "metadata": {
    "original-filename": "architecture-note.txt",
    "content-type": "text/plain",
    "uploaded-utc": "2026-10-16T20:31:00.0000000Z",
    "content-length": "68"
  },
  "title": "Architecture Overview Note",
  "description": "Summary of system architecture and AWS S3 integration.",
  "fileName": "architecture-note.txt",
  "contentType": "text/plain",
  "contentLength": 68,
  "uploadedUtc": "2026-10-16T20:31:00.0000000Z"
}
```

#### cURL Example (JSON Creation)
```bash
curl -i -X POST http://localhost:5180/documents \
  -H "Content-Type: application/json" \
  -d '{"title": "Note Title", "description": "Quick note", "fileName": "note.txt", "contentType": "text/plain", "content": "Sample content"}'
```

---

## 5. Update Document Metadata (`PUT /documents/{id}`)

Modifies the editable metadata (`title` and `description`) of an existing document. Updates the record's `updatedUtc` timestamp.

### Parameters
| Name | In | Type | Required | Description |
| :--- | :--- | :--- | :--- | :--- |
| `id` | path | string | Yes | Document unique identifier |

### Request Body Schema
```json
{
  "title": "Updated Document Title",
  "description": "Updated document description text."
}
```

### Responses
#### Success (`200 OK`)
```json
{
  "id": "doc-1",
  "objectKey": "documents/uploads/2026/10/12/sample-project-spec.pdf",
  "fileName": "project-spec.pdf",
  "contentType": "application/pdf",
  "contentLength": 104857,
  "uploadedUtc": "2026-10-16T13:40:00Z",
  "title": "Updated Document Title",
  "description": "Updated document description text.",
  "updatedUtc": "2026-10-16T20:35:12.4567890Z",
  "metadata": {
    "original-filename": "project-spec.pdf"
  }
}
```

#### Bad Request (`400 Bad Request`)
```json
{
  "errors": [
    "Title cannot be empty."
  ]
}
```

#### Not Found (`404 Not Found`)
```json
{
  "error": "Document with ID 'non-existent-id' was not found."
}
```

### cURL Example
```bash
curl -i -X PUT http://localhost:5180/documents/doc-1 \
  -H "Content-Type: application/json" \
  -d '{"title": "Updated Title v2", "description": "New description"}'
```

---

## 6. Delete Document (`DELETE /documents/{id}`)

Removes the document metadata record from the repository index.

### Parameters
| Name | In | Type | Required | Description |
| :--- | :--- | :--- | :--- | :--- |
| `id` | path | string | Yes | Document unique identifier |

### Responses
#### Success (`200 OK`)
```json
{
  "message": "Document 'doc-2' deleted successfully.",
  "id": "doc-2"
}
```

#### Not Found (`404 Not Found`)
```json
{
  "error": "Document with ID 'non-existent-id' was not found."
}
```

### cURL Example
```bash
curl -i -X DELETE http://localhost:5180/documents/doc-2
```
