import { Component, computed, DestroyRef, inject, OnInit, signal } from '@angular/core';
import { ProfileService } from '../service/profile-service';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { UserResponse } from '../models/profile.model';
import { AuthService } from '../../../core/auth/service/auth-service';
import { Router } from '@angular/router';

@Component({
  selector: 'app-profile',
  imports: [],
  templateUrl: './profile.html',
  styleUrl: './profile.css',
})
export class ProfileComponent implements OnInit {
  private profileService = inject(ProfileService);
  private readonly authSer = inject(AuthService);
  private readonly router = inject(Router);
  private destroyRef = inject(DestroyRef);

  user = signal<UserResponse | null>(null);

  readonly email = computed(() => this.user()?.email ?? "guest@gmail.com")
  readonly fullName = computed(() => {
    const user = this.user();
    return user ? `${user.firstName} ${user.lastName}` : 'Guest';
  });

  readonly bgColor = computed(() => this.stringToColor(this.fullName()));
  readonly initials = computed(() => this.getInitials(this.fullName()));

  isOpen = signal(false);
  toggle() {
    this.isOpen.update(v => !v);
  }

  logout() {
    this.authSer.logout()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => this.router.navigate(['/auth/sign-in']),
        error: err => {
          console.error("Something went wrong ", err);
          this.router.navigate(['/auth/sign-in']);
        }
      })
  }

  ngOnInit(): void {
    this.profileService.getUserProfile()
      .pipe(
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe({
        next: val => this.user.set(val),
        error: err => console.error("Failed to get profile: ", err)
      })
  }

  stringToColor(str: string): string {
    let hash = 0;

    for (let i = 0; i < str.length; i++) {
      hash = str.charCodeAt(i) + ((hash << 5) - hash);
    }

    let color = '#';
    for (let i = 0; i < 3; i++) {
      const value = (hash >> (i * 8)) & 0xff;
      color += value.toString(16).padStart(2, '0');
    }

    return color;
  }

  getInitials(fullName: string) {
    const words = fullName
      .trim()
      .split(/\s+/)
      .filter(Boolean);

    if(words.length === 0) return ''

    if (words.length === 1) return words[0][0].toUpperCase();
    
    return (words[0][0] + words[1][0]).toUpperCase();
  }
}
