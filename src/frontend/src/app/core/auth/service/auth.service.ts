import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { BehaviorSubject, catchError, map, Observable, of, tap } from 'rxjs';

import { apiUrl } from '../../../../environments/environment';
import { AuthResponse, Login, Register } from '../models/user.model';

@Injectable({
  providedIn: 'root',
})
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${apiUrl}/api/auth`;
  private readonly profileUrl = `${apiUrl}/api/profiles`;
  private isAuthenticatedSubject = new BehaviorSubject<boolean>(false);
  isAuthenticated$ = this.isAuthenticatedSubject.asObservable();

  register(request: Register): Observable<AuthResponse> {
    const formData = this.buildRegistrationFormData(request);

    return this.http.post<AuthResponse>(`${this.baseUrl}/register`, formData);
  }

  login(request: Login): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.baseUrl}/login`, request).pipe(
      tap(() => this.isAuthenticatedSubject.next(true))
    );
  }

  logout(): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.baseUrl}/logout`, {}, { withCredentials: true }).pipe(
      tap(() => this.isAuthenticatedSubject.next(false))
    );
  }

  refresh(): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/refresh`, {}, { withCredentials: true }).pipe(
      tap(() => this.isAuthenticatedSubject.next(true)),
    );
  }

  checkAuth(): Observable<boolean> {
     return this.http.get(`${this.profileUrl}/me`, { withCredentials: true }).pipe(
      map(() => true),
      tap(() => this.isAuthenticatedSubject.next(true)),
      catchError(() => {
        this.isAuthenticatedSubject.next(false);
        return of(false);
      })
    );
  }

  markAuthenticated() {
    this.isAuthenticatedSubject.next(true);
  }

  private buildRegistrationFormData(request: Register): FormData {
    const formData = new FormData();

    for (const key of ['email','password','confirmPassword','firstName','lastName'] as const) {
      formData.append(key, request[key]);
    }

    if (request.profileImage && request.profileImage.file) {
      formData.append('profileImage.file', request.profileImage.file);
      formData.append('profileImage.name', request.profileImage.description ?? '');
      formData.append('profileImage.description', request.profileImage.description ?? '');
    }

    return formData;
  }
}