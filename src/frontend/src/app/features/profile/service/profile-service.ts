import { inject, Injectable } from '@angular/core';
import { apiUrl } from '../../../../environments/environment';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { UserDto, UserResponse } from '../models/profile.model';

@Injectable({
  providedIn: 'root',
})
export class ProfileService {
  private readonly baseUrl = `${apiUrl}/api/profile`;
  private http = inject(HttpClient);

  getUserProfile(): Observable<UserResponse> {
    return this.http.get<UserResponse>(`${this.baseUrl}/me`);
  }

  updateUser(data: UserDto): Observable<UserResponse> {
    return this.http.put<UserResponse>(`${this.baseUrl}/update-user`, data);
  }
}
