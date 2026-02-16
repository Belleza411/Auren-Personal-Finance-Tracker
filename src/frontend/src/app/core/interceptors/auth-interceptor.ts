import { HttpErrorResponse, HttpHandlerFn, HttpInterceptorFn, HttpRequest } from '@angular/common/http';
import { inject } from '@angular/core';
import { AuthService } from '../auth/service/auth-service';
import { BehaviorSubject, catchError, filter, switchMap, take, throwError } from 'rxjs';

let refreshing = false;
const refreshSubject = new BehaviorSubject<boolean | null>(null);

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const authService = inject(AuthService);

  const request = req.clone({ withCredentials: true });

  return next(request).pipe(
    catchError((err: HttpErrorResponse) => {
      if(err.status === 401) {
        return handle401Error(request, next, authService);
      }

      return throwError(() => err);
    })
  )
};

const handle401Error = (req: HttpRequest<unknown>, next: HttpHandlerFn, authService: AuthService) => {
  if (!refreshing) {
    refreshing = true;
    refreshSubject.next(null);

    return authService.refresh().pipe(
      switchMap(() => {
        refreshing = false;
        refreshSubject.next(false);
        authService.markAuthenticated();
        return next(req.clone(req));
      }),
      catchError(err => {
        refreshing = false;
        refreshSubject.next(false);
        return throwError(() => err);
      })
    );
  }

  return refreshSubject.pipe(
    filter(status => status === false),
    take(1),
    switchMap(() => next(req))
  );  
}
