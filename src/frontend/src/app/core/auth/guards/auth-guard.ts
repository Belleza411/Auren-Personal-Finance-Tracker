import { HttpClient } from '@angular/common/http';
import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { catchError, map, of } from 'rxjs';
import { apiUrl } from '../../../../environments/environment';

export const authGuard: CanActivateFn = () => {
  const baseUrl = `${apiUrl}/api/profile`;
  const http = inject(HttpClient);
  const router = inject(Router);

  return http.get(`${baseUrl}/me`, { withCredentials: true }).pipe(
    map(() => true),
    catchError(() => of(router.createUrlTree(['/login'])))
  )
};
