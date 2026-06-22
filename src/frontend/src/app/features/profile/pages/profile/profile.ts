import { ChangeDetectionStrategy, Component, computed, DestroyRef, inject, OnInit, signal } from '@angular/core';
import { HlmButtonImports } from '@spartan-ng/helm/button';
import { HlmCard, HlmCardContent } from './../../../../libs/ui/card/src'
import { HlmAvatarImports } from '@spartan-ng/helm/avatar';
import { HlmSeparatorImports } from '@spartan-ng/helm/separator';
import { Router } from '@angular/router';
import { AuthService } from 'src/app/core/auth/service/auth.service';
import { UserResponse } from '../../models/profile.model';
import { ProfileService } from '../../service/profile.service';
import { getInitials } from '../../utils/getInitials';
import { stringToColor } from '../../utils/stringToColor';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { HlmCardImports } from './../../../../libs/ui/card/src';
import { HlmInputImports } from '@spartan-ng/helm/input';
import { HlmFieldImports } from '@spartan-ng/helm/field';
import { HlmSpinnerImports } from './../../../../libs/ui/spinner/src/index';
import { lucideTrash, lucideUpload, lucidePencil } from '@ng-icons/lucide';
import { NgIcon, provideIcons } from '@ng-icons/core';

@Component({
  selector: 'app-profile',
  imports: [
    HlmButtonImports,
    HlmAvatarImports,
    HlmSeparatorImports,
    HlmCardImports,
    HlmInputImports,
    HlmFieldImports,
    HlmSpinnerImports,
    NgIcon
  ],
  providers: [provideIcons({ lucideTrash, lucideUpload, lucidePencil })],
  templateUrl: './profile.html',
  styleUrl: './profile.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class Profile implements OnInit {
  private profileService = inject(ProfileService);
  private readonly authSer = inject(AuthService);
  private readonly router = inject(Router);
  private destroyRef = inject(DestroyRef);

  isEditing = signal(false)
  isLoading = signal(false)
  user = signal<UserResponse | null>(null);

  readonly email = computed(() => this.user()?.email ?? "guest@gmail.com")
  readonly fullName = computed(() => {
    const user = this.user();
    return user ? `${user.firstName} ${user.lastName}` : 'Guest';
  });

  readonly bgColor = computed(() => stringToColor(this.fullName()));
  readonly initials = computed(() => getInitials(this.fullName()));

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

  onSubmit(event: Event) {
    event.preventDefault();
    if (!this.isEditing()) return;
    // save logic here
    this.isEditing.set(false);
  }

  onEdit() {
    this.isEditing.set(true);
  }

  onCancel() {
      this.isEditing.set(false);
  }

  onChangePicture() {
      // trigger file input when ready
  }

  onDeletePicture() {
      // call profile service to remove picture
  }

  onDeleteAccount() {
      // show confirmation dialog before deleting
  }
}
