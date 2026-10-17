import 'zone.js';
import { bootstrapApplication } from '@angular/platform-browser';
import { appConfig } from './app/app.config';
import { App } from './app/app';

/**
 * Client entry point: bootstraps the root standalone App component
 * using application configuration providers.
 */
bootstrapApplication(App, appConfig)
  .catch((err) => console.error('Failed to bootstrap Angular application:', err));
