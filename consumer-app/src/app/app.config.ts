import { ApplicationConfig, provideZoneChangeDetection } from '@angular/core';
import { provideHttpClient, withFetch } from '@angular/common/http';

/**
 * Root application configuration for the Angular standalone application.
 * Configures zone event coalescing for performance optimization and
 * enables HttpClient using the modern browser Fetch API backend.
 */
export const appConfig: ApplicationConfig = {
  providers: [
    provideZoneChangeDetection({ eventCoalescing: true }),
    provideHttpClient(withFetch())
  ]
};
