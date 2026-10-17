/**
 * Represents a document item retrieved from or managed by the document API.
 */
export interface DocumentItem {
  /** Unique document record identifier (e.g. "doc-1" or GUID string). */
  id: string;
  /** Hierarchical S3 object key where the binary file payload is stored. */
  objectKey: string;
  /** Original name of the uploaded file including its file extension. */
  fileName: string;
  /** MIME content type of the stored document (e.g., "text/plain", "application/pdf"). */
  contentType: string;
  /** Size of the document content in bytes. */
  contentLength: number;
  /** ISO 8601 UTC timestamp indicating when the document was initially uploaded. */
  uploadedUtc: string;
  /** Human-readable title of the document. */
  title: string;
  /** Descriptive summary of the document. */
  description: string;
  /** ISO 8601 UTC timestamp of the last metadata update, or undefined if never modified. */
  updatedUtc?: string;
  /** Key-value metadata dictionary attached to the S3 object header. */
  metadata?: Record<string, string>;
}

/**
 * Payload sent when creating a new document via JSON POST (/documents).
 */
export interface CreateDocumentPayload {
  /** Required user-facing title of the document. */
  title: string;
  /** Description summarizing the document content. */
  description: string;
  /** Optional file name; defaults to a generated GUID .txt if omitted. */
  fileName?: string;
  /** Optional MIME type; defaults to "text/plain" if omitted. */
  contentType?: string;
  /** Text content to write into the document file stream. */
  content?: string;
}

/**
 * Payload sent when updating an existing document's metadata via PUT (/documents/{id}).
 */
export interface UpdateDocumentPayload {
  /** Required updated title. */
  title: string;
  /** Updated description. */
  description: string;
}

/**
 * Telemetry log record capturing live HTTP request and response metrics.
 */
export interface HttpLogEntry {
  /** Unique tracking identifier for the log entry. */
  id: string;
  /** Local time formatted string when the request was completed. */
  timestamp: string;
  /** HTTP method executed (GET, POST, PUT, DELETE). */
  method: 'GET' | 'POST' | 'PUT' | 'DELETE';
  /** Target API endpoint path or URL. */
  endpoint: string;
  /** HTTP status code returned (e.g. 200, 201, 400, 404, 500, or 0 on network error). */
  statusCode: number;
  /** HTTP status text or network error description. */
  statusText: string;
  /** Round-trip request duration measured in milliseconds. */
  durationMs: number;
  /** Additional diagnostic details or error messages. */
  details?: string;
  /** Indicates whether the HTTP transaction succeeded (2xx status). */
  success: boolean;
}

