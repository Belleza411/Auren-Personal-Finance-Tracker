import { Component, computed, DestroyRef, inject, OnInit, signal } from '@angular/core';
import { ProfileService } from '../service/profile-service';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { UserResponse } from '../models/profile.model';

@Component({
  selector: 'app-profile',
  imports: [],
  templateUrl: './profile.html',
  styleUrl: './profile.css',
})
export class ProfileComponent implements OnInit {
  private profileService = inject(ProfileService);
  private destroyRef = inject(DestroyRef);

  user = signal<UserResponse | null>({
    email: "aevanazelleb@gmail.com",
    firstName: "Aevan",
    lastName: "Belleza",
    profilePictureUrl: null,
    createdAt: "June 3, 2006",
    lastLoginAt: "February 2, 2026"
  });

  readonly email = computed(() => this.user()?.email)
  readonly fullName = computed(() => {
    const user = this.user();
    return user ? `${user.firstName} ${user.lastName}` : '';
  });

  readonly bgColor = computed(() => this.stringToColor(this.fullName()));
  readonly initials = computed(() => this.getInitials(this.fullName()));

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
