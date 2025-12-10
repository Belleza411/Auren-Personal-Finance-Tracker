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

  register(request: Register, file?: File): Observable<AuthResponse> {
    const formData = this.buildRegistrationFormData(request, file);

    return this.http.post<AuthResponse>(`${this.baseUrl}/register`, formData);
  }

  login(request: Login): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.baseUrl}/login`, request);
  }

  logout(): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.baseUrl}/logout`, {});
  }

  private buildRegistrationFormData(request: Register, file?: File): FormData {
    const formData = new FormData();

    formData.append('Email', request.email);
    formData.append('Password', request.password);
    formData.append('ConfirmPassword', request.confirmPassword);
    formData.append('FirstName', request.firstName);
    formData.append('LastName', request.lastName);

    if (file) {
      formData.append('ProfileImage.File', file);
    }

    if (request.profileImage?.name) {
      formData.append('ProfileImage.Name', request.profileImage.name);
    }

    if (request.profileImage?.description) {
      formData.append('ProfileImage.Description', request.profileImage.description);
    }

    return formData;
  }
}
