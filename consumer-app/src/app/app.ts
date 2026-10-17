import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { DocumentService } from './services/document.service';
import { CreateDocumentPayload, DocumentItem, HttpLogEntry, UpdateDocumentPayload } from './models/document.model';

/**
 * Root component of the Document Management Consumer Application.
 * 
 * Demonstrates:
 * - Full CRUD interactions against the ASP.NET Core REST API (GET, POST, PUT, DELETE).
 * - File upload with MIME and extension checks before S3 streaming.
 * - JSON-based quick text document creation.
 * - In-place editing modal with optimistic validation.
 * - Delete confirmation modal with distinct empty state management.
 * - Real-time API connectivity monitor and live telemetry request log.
 * - Plain text template rendering to guard against XSS vulnerabilities.
 */
@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App implements OnInit {
  /** Array of active document items loaded from the backend API. */
  documents: DocumentItem[] = [];

  /** Currently selected document displayed in the details modal dialog. */
  selectedDocument: DocumentItem | null = null;

  /** Search string filter applied across titles, file names, descriptions, and IDs. */
  searchTerm: string = '';

  /** Indicates whether the document catalog is currently being fetched (GET). */
  isLoading: boolean = false;

  /** Indicates whether an active mutation (POST, PUT, DELETE) is in-flight. */
  isActionLoading: boolean = false;

  /** User-facing error message displayed in the top alert banner. */
  errorMessage: string | null = null;

  /** User-facing success confirmation banner text. */
  successMessage: string | null = null;

  /** Connection state indicating whether /health responded successfully. */
  apiConnected: boolean | null = null;

  /** Editable API base URL bound to the top connection bar. */
  apiUrlInput: string = '';

  /** Active creation tab: 'file' for multipart upload or 'json' for quick text creation. */
  creationMode: 'file' | 'json' = 'file';

  /** Staged file selected from the local file input. */
  selectedFile: File | null = null;

  /** Title input for the multipart upload form. */
  fileTitle: string = '';

  /** Description input for the multipart upload form. */
  fileDescription: string = '';

  /** Model bound to the quick JSON document creation form. */
  newDoc: CreateDocumentPayload = {
    title: '',
    description: '',
    fileName: 'document.txt',
    contentType: 'text/plain',
    content: ''
  };

  /** The document currently being edited inside the edit modal dialog. */
  editingDocument: DocumentItem | null = null;

  /** Form state holding updated title and description for PUT requests. */
  editPayload: UpdateDocumentPayload = {
    title: '',
    description: ''
  };

  /** Document ID currently awaiting deletion confirmation inside the delete modal. */
  deletingId: string | null = null;

  /**
   * Initializes the component and injects the document service.
   * @param documentService Injected REST API client service.
   */
  constructor(public documentService: DocumentService) {}


  /**
   * Angular lifecycle hook executed upon component initialization.
   * Auto-populates the API URL input and verifies backend health.
   */
  ngOnInit(): void {
    this.apiUrlInput = this.documentService.getBaseUrl();
    this.checkHealthAndLoad();
  }

  /**
   * Retrieves the live HTTP activity telemetry log entries.
   */
  get httpLogs(): HttpLogEntry[] {
    return this.documentService.httpLogs;
  }

  /**
   * Returns documents filtered by the current user search term across title,
   * filename, description, and document ID.
   */
  get filteredDocuments(): DocumentItem[] {
    if (!this.searchTerm.trim()) {
      return this.documents;
    }
    const term = this.searchTerm.toLowerCase();
    return this.documents.filter(doc =>
      (doc.title && doc.title.toLowerCase().includes(term)) ||
      (doc.fileName && doc.fileName.toLowerCase().includes(term)) ||
      (doc.description && doc.description.toLowerCase().includes(term)) ||
      (doc.id && doc.id.toLowerCase().includes(term))
    );
  }

  /**
   * Reconfigures the base URL in the document service and re-verifies health.
   */
  applyApiUrl(): void {
    this.documentService.setBaseUrl(this.apiUrlInput);
    this.checkHealthAndLoad();
  }

  /**
   * Executes a GET /health check. If healthy, proceeds to load document catalog;
   * otherwise, displays a connection failure alert banner.
   */
  checkHealthAndLoad(): void {
    this.clearMessages();
    this.documentService.checkHealth().subscribe({
      next: () => {
        this.apiConnected = true;
        this.loadDocuments();
      },
      error: (err) => {
        this.apiConnected = false;
        this.errorMessage = this.documentService.extractErrorMessage(err);
      }
    });
  }

  // ==========================================
  // GET: Fetch list of documents from live API
  // ==========================================

  /**
   * Retrieves the full collection of stored documents from GET /documents.
   */
  loadDocuments(): void {
    this.isLoading = true;
    this.clearMessages();

    this.documentService.getDocuments().subscribe({
      next: (docs) => {
        this.documents = docs;
        this.isLoading = false;
      },
      error: (err) => {
        this.isLoading = false;
        this.errorMessage = this.documentService.extractErrorMessage(err);
      }
    });
  }

  // ==========================================
  // POST: Create / Upload new resource
  // ==========================================

  /**
   * Event handler triggered when a user selects a file from the file input control.
   * Auto-derives the document title from the filename if currently blank.
   * @param event DOM file change event.
   */
  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files.length > 0) {
      this.selectedFile = input.files[0];
      if (!this.fileTitle) {
        this.fileTitle = this.selectedFile.name.replace(/\.[^/.]+$/, '');
      }
    }
  }

  /**
   * Submits a multipart/form-data upload to POST /documents.
   * Validates that a file has been selected, streams to S3 via API, resets form state,
   * and refreshes the document table.
   */
  submitFileUpload(): void {
    if (!this.selectedFile) {
      this.errorMessage = 'Please select a file to upload (.txt, .pdf, .png, .jpg).';
      return;
    }

    this.isActionLoading = true;
    this.clearMessages();

    this.documentService.uploadDocument(this.selectedFile, this.fileTitle, this.fileDescription).subscribe({
      next: (response) => {
        this.isActionLoading = false;
        this.successMessage = `Successfully uploaded "${this.selectedFile?.name}". Resource created!`;
        this.selectedFile = null;
        this.fileTitle = '';
        this.fileDescription = '';
        
        // Reset file input in HTML
        const fileInput = document.getElementById('fileUploadInput') as HTMLInputElement;
        if (fileInput) {
          fileInput.value = '';
        }

        // Refresh documents list from live API (GET)
        this.loadDocuments();
      },
      error: (err) => {
        this.isActionLoading = false;
        this.errorMessage = this.documentService.extractErrorMessage(err);
      }
    });
  }

  /**
   * Submits a JSON-based text document to POST /documents.
   * Validates title presence, creates resource, and triggers table refresh.
   */
  submitJsonDocument(): void {
    if (!this.newDoc.title.trim()) {
      this.errorMessage = 'Document Title is required.';
      return;
    }

    this.isActionLoading = true;
    this.clearMessages();

    this.documentService.createDocumentJson(this.newDoc).subscribe({
      next: () => {
        this.isActionLoading = false;
        this.successMessage = `Document "${this.newDoc.title}" created successfully!`;
        this.newDoc = {
          title: '',
          description: '',
          fileName: 'note.txt',
          contentType: 'text/plain',
          content: ''
        };
        // Refresh live list
        this.loadDocuments();
      },
      error: (err) => {
        this.isActionLoading = false;
        this.errorMessage = this.documentService.extractErrorMessage(err);
      }
    });
  }

  // ==========================================
  // PUT: Edit existing resource
  // ==========================================

  /**
   * Opens the edit modal for a given document item.
   * Copies current title and description into the editable form model.
   * @param doc The document item to modify.
   */
  startEdit(doc: DocumentItem): void {
    this.editingDocument = { ...doc };
    this.editPayload = {
      title: doc.title,
      description: doc.description || ''
    };
    this.clearMessages();
  }

  /**
   * Closes the edit modal without saving changes.
   */
  cancelEdit(): void {
    this.editingDocument = null;
  }

  /**
   * Submits the updated Title and Description to PUT /documents/{id}.
   */
  submitUpdate(): void {
    if (!this.editingDocument) return;

    if (!this.editPayload.title.trim()) {
      this.errorMessage = 'Title cannot be empty.';
      return;
    }

    this.isActionLoading = true;
    this.clearMessages();

    this.documentService.updateDocument(this.editingDocument.id, this.editPayload).subscribe({
      next: (updated) => {
        this.isActionLoading = false;
        this.successMessage = `Document "${updated.title}" updated successfully!`;
        this.editingDocument = null;
        // Refresh live list
        this.loadDocuments();
      },
      error: (err) => {
        this.isActionLoading = false;
        this.errorMessage = this.documentService.extractErrorMessage(err);
      }
    });
  }

  // ==========================================
  // DELETE: Remove resource
  // ==========================================

  /**
   * Opens the confirmation dialog modal for deleting a document.
   * @param doc The document item targeted for deletion.
   */
  confirmDelete(doc: DocumentItem): void {
    this.deletingId = doc.id;
  }

  /**
   * Closes the deletion confirmation modal without deleting.
   */
  cancelDelete(): void {
    this.deletingId = null;
  }

  /**
   * Dispatches the delete request to DELETE /documents/{id}.
   * Closes the modal and triggers a refresh of the document catalog.
   * @param id The ID of the document to remove.
   */
  executeDelete(id: string): void {
    this.isActionLoading = true;
    this.clearMessages();

    this.documentService.deleteDocument(id).subscribe({
      next: () => {
        this.isActionLoading = false;
        this.deletingId = null;
        this.successMessage = `Document successfully deleted.`;
        // Refresh live list to reflect removal
        this.loadDocuments();
      },
      error: (err) => {
        this.isActionLoading = false;
        this.deletingId = null;
        this.errorMessage = this.documentService.extractErrorMessage(err);
      }
    });
  }

  // ==========================================
  // View Details Modal
  // ==========================================

  /**
   * Opens the document detail modal dialog inspecting all metadata and S3 tags.
   * @param doc The document item to inspect.
   */
  viewDetails(doc: DocumentItem): void {
    this.selectedDocument = doc;
  }

  /**
   * Closes the document detail inspection modal dialog.
   */
  closeDetails(): void {
    this.selectedDocument = null;
  }

  // ==========================================
  // Formatters & UI Helpers
  // ==========================================

  /**
   * Clears all user-facing success and error notification banners.
   */
  clearMessages(): void {
    this.errorMessage = null;
    this.successMessage = null;
  }

  /**
   * Formats a raw byte count into human-readable units (Bytes, KB, MB).
   * @param bytes Number of bytes.
   * @returns Formatted size string (e.g. "12.4 KB").
   */
  formatBytes(bytes: number): string {
    if (bytes === 0) return '0 Bytes';
    const k = 1024;
    const sizes = ['Bytes', 'KB', 'MB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i];
  }

  /**
   * Formats an ISO 8601 UTC date string into the user's localized date and time.
   * @param utcDateStr ISO timestamp string.
   * @returns Localized date string.
   */
  formatDate(utcDateStr: string | undefined): string {
    if (!utcDateStr) return 'N/A';
    try {
      const date = new Date(utcDateStr);
      return date.toLocaleString();
    } catch {
      return utcDateStr;
    }
  }
}
