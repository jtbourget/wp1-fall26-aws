export interface DocumentItem {
  id: string;
  objectKey: string;
  fileName: string;
  contentType: string;
  contentLength: number;
  uploadedUtc: string;
  title: string;
  description: string;
  updatedUtc?: string;
  metadata?: Record<string, string>;
}

export interface CreateDocumentPayload {
  title: string;
  description: string;
  fileName?: string;
  contentType?: string;
  content?: string;
}

export interface UpdateDocumentPayload {
  title: string;
  description: string;
}

export interface HttpLogEntry {
  id: string;
  timestamp: string;
  method: 'GET' | 'POST' | 'PUT' | 'DELETE';
  endpoint: string;
  statusCode: number;
  statusText: string;
  durationMs: number;
  details?: string;
  success: boolean;
}
