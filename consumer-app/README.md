# ConsumerApp

This project was generated using [Angular CLI](https://github.com/angular/angular-cli) version 22.2.2.

## Development server

To start a local development server, run:

```bash
ng serve
```

Once the server is running, open your browser and navigate to `http://localhost:4200/`. The application will automatically reload whenever you modify any of the source files.

## Code scaffolding

Angular CLI includes powerful code scaffolding tools. To generate a new component, run:

```bash
ng generate component component-name
```

For a complete list of available schematics (such as `components`, `directives`, or `pipes`), run:

```bash
ng generate --help
```

## Building

To build the project run:

```bash
ng build
```

This will compile your project and store the build artifacts in the `dist/` directory. By default, the production build optimizes your application for performance and speed.

## Running unit tests

To execute unit tests with the [Vitest](https://vitest.dev/) test runner, use the following command:

```bash
ng test
```

## Running end-to-end tests

For end-to-end (e2e) testing, run:

```bash
ng e2e
```

Angular CLI does not come with an end-to-end testing framework by default. You can choose one that suits your needs.

## Additional Resources

For more information on using the Angular CLI, including detailed command references, visit the [Angular CLI Overview and Command Reference](https://angular.dev/tools/cli) page.

---

## 4. Consumed API Routes and Resources

The consumer app communicates with the following REST API routes:

| HTTP Method | Route | Target Resource | Description |
| :--- | :--- | :--- | :--- |
| **GET** | `/documents` | **Document Collection** | Retrieves all stored document items for the data table view. |
| **GET** | `/documents/{id}` | **Individual Document** | Retrieves details for a specific document by its ID or S3 object key. |
| **POST** | `/documents` | **Document Collection** | Submits file uploads (multipart/form-data) or creates text documents (JSON). |
| **PUT** | `/documents/{id}` | **Individual Document** | Updates document metadata (Title, Description). |
| **DELETE** | `/documents/{id}` | **Individual Document** | Deletes a document by ID. |
| **GET** | `/health` | **Service Health** | Health check verifying API connectivity and displaying connection status. |

