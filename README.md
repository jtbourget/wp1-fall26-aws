# Wp1Fall26Aws

A cloud-connected document storage API built with ASP.NET Core (.NET 10) and Amazon S3, featuring an Angular consumer client application that demonstrates full CRUD capabilities (GET, POST, PUT, DELETE).

---

## 4. API Routes and Resources

The ASP.NET Core API provides full RESTful endpoints to manage document resources and upload files to Amazon S3. All endpoints support cross-origin requests (CORS).

| HTTP Method | Route | Target Resource | Request Payload / Format | Status Codes | Description |
| :--- | :--- | :--- | :--- | :--- | :--- |
| **GET** | `/documents` | **Document Collection** | *None* | `200 OK` | Retrieves a list of all stored document records, ordered by newest first. |
| **GET** | `/documents/{id}` | **Individual Document** | *None* (ID or S3 ObjectKey in path) | `200 OK`<br>`404 Not Found` | Retrieves detailed metadata for a specific document by its unique ID or S3 object key. |
| **POST** | `/documents` | **Document Collection** | **Multipart Form:** `file` (binary, max 1 MiB), `title` (optional), `description` (optional)<br>— OR —<br>**JSON:** `title`, `fileName`, `contentType`, `content`, `description` | `201 Created`<br>`400 Bad Request`<br>`500 Server Error` | Creates a new document resource and persists the file stream into Amazon S3 with custom object metadata (e.g. `original-filename`). |
| **PUT** | `/documents/{id}` | **Individual Document** | **JSON:** `title` (required), `description` (optional) | `200 OK`<br>`400 Bad Request`<br>`404 Not Found` | Updates editable metadata fields (`Title`, `Description`) of an existing document record. Sets `UpdatedUtc`. |
| **DELETE** | `/documents/{id}` | **Individual Document** | *None* (ID in path) | `200 OK`<br>`404 Not Found` | Deletes a document record from the storage repository. |
| **GET** | `/health` | **Service Health** | *None* | `200 OK` | Liveness check returning `{ "status": "Healthy" }`. Used by consumer app to verify connectivity. |

---

## Running Locally

### Prerequisites
* [.NET 10 SDK](https://dotnet.microsoft.com/)
* [Node.js 20 LTS+](https://nodejs.org/) & `npm`

### 1. Run the ASP.NET Core API
```powershell
dotnet run --project src/Wp1Fall26Aws.Api --urls "http://localhost:5180"
```
The API listens at `http://localhost:5180`.

### 2. Run the Angular Consumer App
```powershell
cd consumer-app
npm install
npm start
```
The client serves at `http://localhost:4200` and automatically connects to the backend API.

---

## Storage Behavior & S3 Configuration

Files uploaded via `POST /documents` are validated and written to AWS S3.

### Validation Policy
* **Maximum File Size:** 1 MiB (1,048,576 bytes)
* **Allowed Extensions:** `.txt`, `.pdf`, `.png`, `.jpg`, `.jpeg`
* **Allowed Content Types:** `text/plain`, `application/pdf`, `image/png`, `image/jpeg`
* **Zero-byte / Empty Files:** Rejected (`400 Bad Request`)

### Key Format
S3 object keys are generated using date partitioning and GUID uniqueness:
```text
{KeyPrefix}/{yyyy}/{MM}/{dd}/{guid:N}{.ext}
```

### AWS Configuration Keys
* `S3Storage:BucketName`
* `S3Storage:KeyPrefix`
* `S3Storage:Region`
* `S3Storage:MaxFileSizeBytes`

To configure AWS S3 for local testing:
```bash
dotnet user-secrets --project src/Wp1Fall26Aws.Api set "S3Storage:BucketName" "<your-bucket-name>"
```
*(When no AWS credentials are provided in development, the API uses a safe local mock client).*

---

## Test Suite

Run unit and integration tests with:
```powershell
dotnet test
```
The test suite includes validation tests, boundary tests, spy-based AWS S3 mock tests, and integration tests verifying HTTP endpoints.
