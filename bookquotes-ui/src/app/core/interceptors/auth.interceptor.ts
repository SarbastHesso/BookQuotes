import { HttpInterceptorFn } from '@angular/common/http';
import { environment } from '../../../environments/environment';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const isApiRequest = req.url.startsWith('/api') || req.url.startsWith(`${environment.apiBaseUrl}/api`);

  if (isApiRequest) {
    req = req.clone({
      withCredentials: true,
    });
  }

  return next(req);
};
