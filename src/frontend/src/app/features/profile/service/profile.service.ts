import { inject, Service, signal } from '@angular/core';
import { apiUrl } from '../../../../environments/environment';
import { HttpClient } from '@angular/common/http';
import { Observable, switchMap, tap } from 'rxjs';
import { UserResponse } from '../models/profile.model';

@Service()
export class ProfileService {
  private readonly baseUrl = `${apiUrl}/api/profiles`;
  private http = inject(HttpClient);

  private _user = signal<UserResponse | null>(null);
  readonly user = this._user.asReadonly();

  loadProfile() {
    this.getUserProfile()
      .subscribe({
        next: val => this._user.set(val),
        error: err => console.error('Failed to get profile:', err)
      });
  }

  getUserProfile(): Observable<UserResponse> {
    return this.http.get<UserResponse>(`${this.baseUrl}/me`).pipe(
      tap(user => this._user.set(user))
    );
  }

  updateUser(data: FormData) {
    return this.http.put<void>(`${this.baseUrl}/update-user`, data).pipe(
        switchMap(() => this.getUserProfile())
    );
}
}
