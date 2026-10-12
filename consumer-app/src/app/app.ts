import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { DocumentService } from './services/document.service';
import { CreateDocumentPayload, DocumentItem, HttpLogEntry, UpdateDocumentPayload } from './models/document.model';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App implements OnInit {
  // State
  documents: DocumentItem[] = [];
  selectedDocument: DocumentItem | null = null;
  searchTerm: string = '';

  // Request States
  isLoading: boolean = false;
  isActionLoading: boolean = false;
  errorMessage: string | null = null;
  successMessage: string | null = null;

  // API connectivity
  apiConnected: boolean | null = null;
  apiUrlInput: string = '';

  // Create Mode (Tab)
  creationMode: 'file' | 'json' = 'file';

  // File Upload Form
  selectedFile: File | null = null;
  fileTitle: string = '';
  fileDescription: string = '';

  // JSON Quick Document Form
  newDoc: CreateDocumentPayload = {
    title: '',
    description: '',
    fileName: 'document.txt',
    contentType: 'text/plain',
    content: ''
  };

  // Edit Mode Form
  editingDocument: DocumentItem | null = null;
  editPayload: UpdateDocumentPayload = {
    title: '',
    description: ''
  };

  // Delete confirmation
  deletingId: string | null = null;

  constructor(public documentService: DocumentService) {}

  ngOnInit(): void {
    this.apiUrlInput = this.documentService.getBaseUrl();
    this.checkHealthAndLoad();
  }

  get httpLogs(): HttpLogEntry[] {
    return this.documentService.httpLogs;
  }

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

  applyApiUrl(): void {
    this.documentService.setBaseUrl(this.apiUrlInput);
    this.checkHealthAndLoad();
  }

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
  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files.length > 0) {
      this.selectedFile = input.files[0];
      if (!this.fileTitle) {
        this.fileTitle = this.selectedFile.name.replace(/\.[^/.]+$/, '');
      }
    }
  }

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
  startEdit(doc: DocumentItem): void {
    this.editingDocument = { ...doc };
    this.editPayload = {
      title: doc.title,
      description: doc.description || ''
    };
    this.clearMessages();
  }

  cancelEdit(): void {
    this.editingDocument = null;
  }

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
  confirmDelete(doc: DocumentItem): void {
    this.deletingId = doc.id;
  }

  cancelDelete(): void {
    this.deletingId = null;
  }

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
  viewDetails(doc: DocumentItem): void {
    this.selectedDocument = doc;
  }

  closeDetails(): void {
    this.selectedDocument = null;
  }

  // Helpers
  clearMessages(): void {
    this.errorMessage = null;
    this.successMessage = null;
  }

  formatBytes(bytes: number): string {
    if (bytes === 0) return '0 Bytes';
    const k = 1024;
    const sizes = ['Bytes', 'KB', 'MB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i];
  }

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
