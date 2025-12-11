import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';

import { apiUrl } from '../../../../environments/environment';
import { AuthResponse, Login, Register } from '../models/user.model';

@Injectable({
  providedIn: 'root',
})
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${apiUrl}/api/auth`;

  register(request: Register): Observable<AuthResponse> {
    const formData = this.buildRegistrationFormData(request);

    return this.http.post<AuthResponse>(`${this.baseUrl}/register`, formData);
  }

  login(request: Login): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.baseUrl}/login`, request);
  }

  logout(): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.baseUrl}/logout`, {});
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
