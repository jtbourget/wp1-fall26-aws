# Document Management Consumer Application (Angular 22)

A modern, responsive Single-Page Application (SPA) built with **Angular 22** that provides an interactive user interface for managing documents stored in Amazon S3 via the **Wp1Fall26Aws** ASP.NET Core REST API.

---

## 1. Features & Capabilities

- **Full RESTful CRUD Implementation**:
  - **GET**: Lists stored documents with loading spinners, search filtering, and detailed metadata modal views.
  - **POST**: Supports two creation workflows:
    - *Multipart File Upload*: Native drag-and-drop or file picker with auto-derived titles and file size formatting.
    - *Quick Text Document*: Instant text note creation serialized into JSON payloads.
  - **PUT**: In-place metadata editing modal allowing live updates of document Title and Description with optimistic UI refresh.
  - **DELETE**: Safe deletion flow with confirmation modal and graceful handling of empty catalog states.
- **Live HTTP Activity & Telemetry Log**:
  - Displays real-time HTTP metrics at the bottom of the dashboard: HTTP method badge, status code, round-trip latency in milliseconds, target endpoint, and diagnostic details.
  - Stores a rolling window of the last 30 requests.
- **Runtime API Connection Switcher**:
  - Auto-detects the backend API at `http://localhost:5180` when running locally under `ng serve` on port 4200.
  - When served from production (co-hosted in ASP.NET Core `wwwroot`), automatically uses the origin server.
  - Allows manual URL override directly in the header with live health checking (`GET /health`).
- **Security & XSS Mitigation**:
  - Employs strict Angular text interpolation expressions (`{{ doc.title }}`) to prevent unescaped HTML/script execution.
  - Validates file selection client-side before transmission.

---

## 2. Project Structure

```text
consumer-app/
├── public/                 # Static public assets (favicon, icons)
├── src/
│   ├── app/
│   │   ├── models/
│   │   │   └── document.model.ts      # TypeScript interfaces (DocumentItem, CreatePayload, HttpLogEntry)
│   │   ├── services/
│   │   │   └── document.service.ts    # REST API client with telemetry interceptors and error parsers
│   │   ├── app.config.ts              # Angular application providers (zone coalescing, fetch backend)
│   │   ├── app.css                    # Component-scoped styling and responsive dashboard layout
│   │   ├── app.html                   # HTML template with forms, modals, table, and telemetry log
│   │   └── app.ts                     # Root standalone App component state machine and event handlers
│   ├── index.html                     # HTML root entry point
│   ├── main.ts                        # Application bootstrap script
│   └── styles.css                     # Global styles and CSS utility classes
├── angular.json                       # Angular CLI workspace configuration
├── package.json                       # NPM dependencies and project scripts
└── tsconfig.json                      # TypeScript compiler configuration
```

---

## 3. Getting Started & Development

### Prerequisites
- [Node.js 20 LTS+](https://nodejs.org/)
- `npm` (bundled with Node.js)

### 1. Install Dependencies
```bash
cd consumer-app
npm install
```

### 2. Start the Development Server
```bash
npm start
# or: ng serve
```
The application starts at **`http://localhost:4200`** with hot module replacement (HMR).

> [!NOTE]
> Make sure the ASP.NET Core API is running at `http://localhost:5180` before testing endpoints:
> ```powershell
> dotnet run --project src/Wp1Fall26Aws.Api --urls "http://localhost:5180"
> ```

### 3. Build for Production
```bash
npm run build
```
This builds an optimized production bundle directly into the ASP.NET Core static files directory:
`../src/Wp1Fall26Aws.Api/wwwroot`

When published, the .NET Web API serves the Angular SPA directly from `wwwroot` without requiring Node.js in production.

---

## 4. Consumed REST API Routes

The Angular client communicates with the following endpoints:

| HTTP Method | Route | Target Resource | Description |
| :--- | :--- | :--- | :--- |
| **GET** | `/health` | Service Health | Health check verifying connectivity and updating the status indicator. |
| **GET** | `/documents` | Document Collection | Retrieves all stored documents ordered newest first. |
| **GET** | `/documents/{id}` | Individual Document | Retrieves metadata for an individual document. |
| **POST** | `/documents` | Document Collection | Submits file uploads (multipart) or creates text documents (JSON). |
| **PUT** | `/documents/{id}` | Individual Document | Updates editable metadata fields (`Title`, `Description`). |
| **DELETE** | `/documents/{id}` | Individual Document | Deletes a document by ID and refreshes the catalog. |
