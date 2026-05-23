import { ApplicationConfig, provideBrowserGlobalErrorListeners } from '@angular/core';
import { provideRouter, withRouterConfig } from '@angular/router';
import { provideHttpClient, withInterceptors } from '@angular/common/http';

import { routes } from './app.routes';
import { authInterceptor } from './core/interceptors/auth.interceptor';

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideRouter(routes, withRouterConfig({
      // Ensure SPA navigation always scrolls to top on new navigation
      scrollPositionRestoration: 'enabled',
      // Enable anchor fragment scrolling
      anchorScrolling: 'enabled',
      // Offset to account for the sticky navbar height (approx. 56px)
      scrollOffset: [0, 56],
    })),
    provideHttpClient(withInterceptors([authInterceptor])),
  ],
};
