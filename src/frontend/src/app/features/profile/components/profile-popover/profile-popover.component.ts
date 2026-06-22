import { Component, computed, DestroyRef, inject, OnInit, signal } from '@angular/core';
import { ProfileService } from '../../service/profile.service';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { UserResponse } from '../../models/profile.model';
import { AuthService } from '../../../../core/auth/service/auth.service';
import { Router, RouterLink } from '@angular/router';
import { HlmAvatar, HlmAvatarFallback, HlmAvatarImage } from "../../../../libs/ui/avatar/src";
import { HlmPopoverImports } from '@spartan-ng/helm/popover';
import { BrnPopoverContent } from '@spartan-ng/brain/popover';
import { HlmSeparatorImports } from '@spartan-ng/helm/separator';
import { lucideChevronDown, lucideLogOut, lucideUser, lucideSettings } from '@ng-icons/lucide';
import { NgIcon, provideIcons } from '@ng-icons/core';
import { HlmButtonImports } from '@spartan-ng/helm/button';
import { stringToColor } from '../../utils/stringToColor';
import { getInitials } from '../../utils/getInitials';

@Component({
  selector: 'app-profile',
  imports: [
    HlmAvatar,
    HlmAvatarFallback,
    HlmAvatarImage, 
    HlmPopoverImports,
    BrnPopoverContent,
    HlmSeparatorImports,
    NgIcon,
    RouterLink,
    HlmButtonImports
  ],
  providers: [provideIcons({ lucideChevronDown, lucideLogOut, lucideUser, lucideSettings })],
  templateUrl: './profile-popover.html',
  styleUrl: './profile-popover.css',
})
export class ProfilePopover implements OnInit {
  private profileService = inject(ProfileService);
  private readonly authSer = inject(AuthService);
  private readonly router = inject(Router);
  private destroyRef = inject(DestroyRef);

  readonly user = this.profileService.user;

  readonly email = computed(() => this.user()?.email ?? "guest@gmail.com")
  readonly fullName = computed(() => {
    const user = this.user();
    return user ? `${user.firstName} ${user.lastName}` : 'Guest';
  });

  readonly bgColor = computed(() => stringToColor(this.fullName()));
  readonly initials = computed(() => getInitials(this.fullName()));

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
    this.profileService.loadProfile();
  } 
}
