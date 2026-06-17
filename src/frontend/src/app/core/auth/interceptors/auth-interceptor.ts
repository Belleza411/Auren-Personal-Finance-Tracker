import { HttpErrorResponse, HttpHandlerFn, HttpInterceptorFn, HttpRequest } from '@angular/common/http';
import { inject } from '@angular/core';
import { AuthService } from '../service/auth.service';
import { BehaviorSubject, catchError, filter, switchMap, take, throwError } from 'rxjs';

type RefreshStatus = 'success' | 'failed' | null;

let isRefreshing = false;
const refreshSubject = new BehaviorSubject<RefreshStatus>(null);

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const authService = inject(AuthService);

  const request = req.clone({ withCredentials: true });

  const isAuthRequest =
    req.url.includes('/auth/login') ||
    req.url.includes('/auth/register') ||
    req.url.includes('/auth/refresh');

  return next(request).pipe(
    catchError((err: HttpErrorResponse) => {
      if(err.status === 401 && !isAuthRequest) {
        return handle401Error(request, next, authService);
      }

      return throwError(() => err);
    })
  )
};

const handle401Error = (req: HttpRequest<unknown>, next: HttpHandlerFn, authService: AuthService) => {
  if (!isRefreshing) {
    isRefreshing = true;
    refreshSubject.next(null);

    return authService.refresh().pipe(
      switchMap(() => {
        isRefreshing = false;
        refreshSubject.next('success');
        authService.markAuthenticated();
        return next(req);
      }),
      catchError(err => {
        isRefreshing = false;
        refreshSubject.next('failed');
        authService.logout();
        return throwError(() => err);
      })
    );
  }

  return refreshSubject.pipe(
    filter(status => status !== null),
    take(1),
    switchMap(status => {
      if (status === 'success') {
        return next(req); 
      }

      return throwError(() => new Error('Session expired. Please log in again.'));
    })
  );  
}
