import { HttpInterceptorFn, HttpRequest, HttpHandlerFn, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { AuthService } from '../services/auth.service';
import { catchError, switchMap, throwError } from 'rxjs';

export const authInterceptor: HttpInterceptorFn = (req: HttpRequest<unknown>, next: HttpHandlerFn) => {
  const authService = inject(AuthService);
  const token = authService.token();

  // Add token to request if available
  let authReq = req;
  if (token) {
    authReq = req.clone({
      setHeaders: {
        Authorization: `Bearer ${token}`
      }
    });
  }

  return next(authReq).pipe(
    catchError((error: HttpErrorResponse) => {
      // Handle 401 errors - try to refresh token
      if (error.status === 401 && token) {
        return authService.refreshToken().pipe(
          switchMap(response => {
            if (response) {
              // Retry the request with new token
              const retryReq = req.clone({
                setHeaders: {
                  Authorization: `Bearer ${response.token}`
                }
              });
              return next(retryReq);
            }
            return throwError(() => error);
          })
        );
      }
      return throwError(() => error);
    })
  );
};
