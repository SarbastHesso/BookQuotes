import { HttpInterceptorFn } from '@angular/common/http';
import { environment } from '../../../environments/environment';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const isApiRequest = req.url.startsWith('/api') || req.url.startsWith(`${environment.apiBaseUrl}/api`);

  if (isApiRequest) {
    const token = localStorage.getItem('bookquotes_auth_token');

    req = req.clone({
      withCredentials: true,
      setHeaders: token ? { Authorization: `Bearer ${token}` } : undefined,
    });
  }

  return next(req);
};
