import { ApplicationConfig, provideBrowserGlobalErrorListeners } from '@angular/core';
import { provideRouter, withComponentInputBinding } from '@angular/router';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { provideToastr } from 'ngx-toastr';

import { routes } from './app.routes';
import { authInterceptor } from './core/auth/interceptors/auth-interceptor';
import { OVERLAY_DEFAULT_CONFIG } from '@angular/cdk/overlay';

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideRouter(routes, withComponentInputBinding()),
    provideHttpClient(
      withInterceptors([authInterceptor])
    ),
    provideToastr({
      positionClass: 'toast-top-right',
      preventDuplicates: true
    }),
    {
      provide: OVERLAY_DEFAULT_CONFIG,
      useValue: {
        usePopover: false
      }
    }
  ]
};
