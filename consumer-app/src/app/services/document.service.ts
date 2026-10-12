import { Injectable } from '@angular/core';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Observable, catchError, map, throwError } from 'rxjs';
import { CreateDocumentPayload, DocumentItem, HttpLogEntry, UpdateDocumentPayload } from '../models/document.model';

@Injectable({
  providedIn: 'root'
})
export class DocumentService {
  private apiBaseUrl = 'http://localhost:5180';
  public httpLogs: HttpLogEntry[] = [];

  constructor(private http: HttpClient) {}

  getBaseUrl(): string {
    return this.apiBaseUrl;
  }

  setBaseUrl(url: string): void {
    this.apiBaseUrl = url.replace(/\/+$/, '');
  }

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
