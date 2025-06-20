import { ApplicationConfig, inject,
  provideAppInitializer, provideBrowserGlobalErrorListeners, provideZonelessChangeDetection } from '@angular/core';
import {provideRouter, withComponentInputBinding} from '@angular/router';
import { routes } from './app.routes';
import {provideHttpClient} from '@angular/common/http';
import {ConfigService} from '../core/services/config-service';
import {HashLocationStrategy, LocationStrategy} from '@angular/common';

function loadAppConfig(configService: ConfigService): () => Promise<void> {
  return async () => await configService.loadConfig();
}

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideZonelessChangeDetection(),
    provideRouter(routes, withComponentInputBinding()),
    provideHttpClient(),
    provideAppInitializer(() => loadAppConfig(inject(ConfigService))()),
    {provide: LocationStrategy, useClass: HashLocationStrategy}
  ]
};
