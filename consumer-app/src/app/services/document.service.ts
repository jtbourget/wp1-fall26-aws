import { Injectable } from '@angular/core';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Observable, catchError, map, throwError } from 'rxjs';
import { CreateDocumentPayload, DocumentItem, HttpLogEntry, UpdateDocumentPayload } from '../models/document.model';

/**
 * Service providing RESTful communication with the document backend API.
 * 
 * Features:
 * - Dynamic backend URL detection (auto-resolves localhost:5180 when running under ng serve on 4200).
 * - Real-time HTTP activity and telemetry logging for demonstration and debugging.
 * - Reactive Observable streams for CRUD endpoints (GET, POST, PUT, DELETE).
 * - Centralized HTTP error normalization and user-friendly error message extraction.
 */
@Injectable({
  providedIn: 'root'
})
export class DocumentService {
  /** The base URL of the ASP.NET Core API backend. */
  private apiBaseUrl: string;

  /** Circular buffer of recent HTTP requests with duration and status details. */
  public httpLogs: HttpLogEntry[] = [];

  constructor(private http: HttpClient) {
    this.apiBaseUrl = this.detectDefaultBaseUrl();
  }

  /**
   * Intelligently discovers the default API base URL based on browser origin.
   * If running locally via Angular CLI dev server on port 4200, points to the .NET API at http://localhost:5180.
   * Otherwise, assumes the API is co-hosted with the frontend under the same origin.
   */
  private detectDefaultBaseUrl(): string {
    if (typeof window !== 'undefined' && window.location) {
      if (window.location.port === '4200') {
        return 'http://localhost:5180';
      }
      return window.location.origin;
    }
    return 'http://localhost:5180';
  }

  /**
   * Returns the currently configured API base URL.
   */
  getBaseUrl(): string {
    return this.apiBaseUrl;
  }

  /**
   * Overrides the API base URL at runtime (e.g. from the UI connection bar).
   * Automatically strips trailing slashes for consistency.
   * @param url The new base URL string.
   */
  setBaseUrl(url: string): void {
    this.apiBaseUrl = url.replace(/\/+$/, '');
  }

  /**
   * Appends an entry to the rolling HTTP activity log, keeping the most recent 30 events.
   * @param entry Log entry without ID and timestamp (auto-populated).
   */
  private addLog(entry: Omit<HttpLogEntry, 'id' | 'timestamp'>): void {
    const log: HttpLogEntry = {
      ...entry,
      id: Math.random().toString(36).substring(2, 9),
      timestamp: new Date().toLocaleTimeString()
    };
    this.httpLogs.unshift(log);
    if (this.httpLogs.length > 30) {
      this.httpLogs.pop();
    }
  }

  /**
   * Performs a health check GET request to /health to verify API connectivity.
   * @returns An Observable emitting `{ status: string }`.
   */
  checkHealth(): Observable<{ status: string }> {
    const start = performance.now();
    const url = `${this.apiBaseUrl}/health`;
    return this.http.get<{ status: string }>(url).pipe(
      map(res => {
        this.addLog({
          method: 'GET',
          endpoint: '/health',
          statusCode: 200,
          statusText: 'OK',
          durationMs: Math.round(performance.now() - start),
          details: `API Status: ${res.status}`,
          success: true
        });
        return res;
      }),
      catchError((err: HttpErrorResponse) => {
        this.addLog({
          method: 'GET',
          endpoint: '/health',
          statusCode: err.status || 0,
          statusText: err.statusText || 'Network Error',
          durationMs: Math.round(performance.now() - start),
          details: err.message,
          success: false
        });
        return throwError(() => err);
      })
    );
  }

  /**
   * Fetches all stored document records from GET /documents.
   * @returns An Observable emitting an array of `DocumentItem` records.
   */
  getDocuments(): Observable<DocumentItem[]> {
    const start = performance.now();
    const url = `${this.apiBaseUrl}/documents`;
    return this.http.get<DocumentItem[]>(url).pipe(
      map(docs => {
        this.addLog({
          method: 'GET',
          endpoint: '/documents',
          statusCode: 200,
          statusText: 'OK',
          durationMs: Math.round(performance.now() - start),
          details: `Retrieved ${docs.length} documents`,
          success: true
        });
        return docs;
      }),
      catchError((err: HttpErrorResponse) => {
        this.addLog({
          method: 'GET',
          endpoint: '/documents',
          statusCode: err.status || 0,
          statusText: err.statusText || 'Network Error',
          durationMs: Math.round(performance.now() - start),
          details: this.extractErrorMessage(err),
          success: false
        });
        return throwError(() => err);
      })
    );
  }

  /**
   * Fetches an individual document record by ID or S3 object key from GET /documents/{id}.
   * @param id The document ID or S3 key.
   * @returns An Observable emitting the matching `DocumentItem`.
   */
  getDocument(id: string): Observable<DocumentItem> {
    const start = performance.now();
    const url = `${this.apiBaseUrl}/documents/${encodeURIComponent(id)}`;
    return this.http.get<DocumentItem>(url).pipe(
      map(doc => {
        this.addLog({
          method: 'GET',
          endpoint: `/documents/${id}`,
          statusCode: 200,
          statusText: 'OK',
          durationMs: Math.round(performance.now() - start),
          details: `Fetched document: ${doc.title}`,
          success: true
        });
        return doc;
      }),
      catchError((err: HttpErrorResponse) => {
        this.addLog({
          method: 'GET',
          endpoint: `/documents/${id}`,
          statusCode: err.status || 0,
          statusText: err.statusText || 'Network Error',
          durationMs: Math.round(performance.now() - start),
          details: this.extractErrorMessage(err),
          success: false
        });
        return throwError(() => err);
      })
    );
  }

  /**
   * Uploads a file via multipart/form-data to POST /documents.
   * The file stream will be persisted into Amazon S3 by the backend.
   * @param file The browser File object to upload.
   * @param title Optional title override.
   * @param description Optional description text.
   * @returns An Observable emitting the newly created resource details.
   */
  uploadDocument(file: File, title: string, description: string): Observable<any> {
    const start = performance.now();
    const url = `${this.apiBaseUrl}/documents`;
    const formData = new FormData();
    formData.append('file', file, file.name);
    if (title.trim()) {
      formData.append('title', title.trim());
    }
    if (description.trim()) {
      formData.append('description', description.trim());
    }

    return this.http.post<any>(url, formData).pipe(
      map(res => {
        this.addLog({
          method: 'POST',
          endpoint: '/documents',
          statusCode: 201,
          statusText: 'Created',
          durationMs: Math.round(performance.now() - start),
          details: `Uploaded file '${file.name}' (${file.size} B)`,
          success: true
        });
        return res;
      }),
      catchError((err: HttpErrorResponse) => {
        this.addLog({
          method: 'POST',
          endpoint: '/documents',
          statusCode: err.status || 0,
          statusText: err.statusText || 'Error',
          durationMs: Math.round(performance.now() - start),
          details: this.extractErrorMessage(err),
          success: false
        });
        return throwError(() => err);
      })
    );
  }

  /**
   * Creates a text-based document from raw JSON payload to POST /documents.
   * @param payload Document metadata and string content.
   * @returns An Observable emitting the created resource details.
   */
  createDocumentJson(payload: CreateDocumentPayload): Observable<any> {
    const start = performance.now();
    const url = `${this.apiBaseUrl}/documents`;
    return this.http.post<any>(url, payload).pipe(
      map(res => {
        this.addLog({
          method: 'POST',
          endpoint: '/documents',
          statusCode: 201,
          statusText: 'Created',
          durationMs: Math.round(performance.now() - start),
          details: `Created document '${payload.title}'`,
          success: true
        });
        return res;
      }),
      catchError((err: HttpErrorResponse) => {
        this.addLog({
          method: 'POST',
          endpoint: '/documents',
          statusCode: err.status || 0,
          statusText: err.statusText || 'Error',
          durationMs: Math.round(performance.now() - start),
          details: this.extractErrorMessage(err),
          success: false
        });
        return throwError(() => err);
      })
    );
  }

  /**
   * Updates an existing document's Title and Description via PUT /documents/{id}.
   * @param id The ID of the document to update.
   * @param payload The updated title and description.
   * @returns An Observable emitting the updated `DocumentItem`.
   */
  updateDocument(id: string, payload: UpdateDocumentPayload): Observable<DocumentItem> {
    const start = performance.now();
    const url = `${this.apiBaseUrl}/documents/${encodeURIComponent(id)}`;
    return this.http.put<DocumentItem>(url, payload).pipe(
      map(updated => {
        this.addLog({
          method: 'PUT',
          endpoint: `/documents/${id}`,
          statusCode: 200,
          statusText: 'OK',
          durationMs: Math.round(performance.now() - start),
          details: `Updated title to '${updated.title}'`,
          success: true
        });
        return updated;
      }),
      catchError((err: HttpErrorResponse) => {
        this.addLog({
          method: 'PUT',
          endpoint: `/documents/${id}`,
          statusCode: err.status || 0,
          statusText: err.statusText || 'Error',
          durationMs: Math.round(performance.now() - start),
          details: this.extractErrorMessage(err),
          success: false
        });
        return throwError(() => err);
      })
    );
  }

  /**
   * Deletes a document record by ID via DELETE /documents/{id}.
   * @param id The ID of the document to delete.
   * @returns An Observable confirming deletion.
   */
  deleteDocument(id: string): Observable<any> {
    const start = performance.now();
    const url = `${this.apiBaseUrl}/documents/${encodeURIComponent(id)}`;
    return this.http.delete<any>(url).pipe(
      map(res => {
        this.addLog({
          method: 'DELETE',
          endpoint: `/documents/${id}`,
          statusCode: 200,
          statusText: 'OK',
          durationMs: Math.round(performance.now() - start),
          details: `Deleted document ID ${id}`,
          success: true
        });
        return res;
      }),
      catchError((err: HttpErrorResponse) => {
        this.addLog({
          method: 'DELETE',
          endpoint: `/documents/${id}`,
          statusCode: err.status || 0,
          statusText: err.statusText || 'Error',
          durationMs: Math.round(performance.now() - start),
          details: this.extractErrorMessage(err),
          success: false
        });
        return throwError(() => err);
      })
    );
  }

  /**
   * Parses an HTTP error response and extracts a user-friendly error message.
   * Handles network disconnects (status 0), structured JSON error lists, and string payloads.
   * @param err The HttpErrorResponse object.
   * @returns Formatted error string for UI alerts.
   */
  public extractErrorMessage(err: HttpErrorResponse): string {
    if (err.status === 0) {
      return `Network connection failed. Make sure the API is running at ${this.apiBaseUrl}.`;
    }
    if (err.error) {
      if (typeof err.error === 'string') {
        return err.error;
      }
      if (err.error.errors && Array.isArray(err.error.errors)) {
        return err.error.errors.join(', ');
      }
      if (err.error.error) {
        return err.error.error;
      }
      if (err.error.message) {
        return err.error.message;
      }
    }
    return `HTTP ${err.status}: ${err.statusText || 'Request failed'}`;
  }
}
