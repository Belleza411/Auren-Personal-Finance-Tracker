import { ChangeDetectionStrategy, Component, computed, inject, input, OnInit, output, signal, viewChild } from '@angular/core';
import { HlmAlertDialogImports } from './../../../../libs/ui/alert-dialog/src';
import { HlmAvatarImports } from '@spartan-ng/helm/avatar';
import { HlmButtonImports } from '@spartan-ng/helm/button';
import { HlmInputImports } from '@spartan-ng/helm/input';
import { HlmLabelImports } from '@spartan-ng/helm/label';
import { NgIcon, provideIcons } from '@ng-icons/core';
import { lucideTrash2 } from '@ng-icons/lucide';
import { stringToColor } from '../../utils/stringToColor';
import { AuthService } from 'src/app/core/auth/service/auth.service';
import { Router } from '@angular/router';
import { AlertService } from 'src/app/core/services/alert.service';
import { ProfileService } from '../../service/profile.service';
import { getInitials } from '../../utils/getInitials';
import { HlmFieldImports } from '@spartan-ng/helm/field';

@Component({
  selector: 'app-delete-account',
  imports: [
    HlmAlertDialogImports,
    HlmButtonImports,
    HlmAvatarImports,
    HlmInputImports,
    HlmLabelImports,
    HlmFieldImports,
    NgIcon
  ],
  providers: [provideIcons({ lucideTrash2 })],
  template: `
    <hlm-alert-dialog 
      [state]="state()" 
      (stateChanged)="onStateChanged($event)">
      <hlm-alert-dialog-content *hlmAlertDialogPortal="let ctx" size="sm">
        <hlm-alert-dialog-header>
          <hlm-alert-dialog-media
            class="bg-destructive/10 text-destructive dark:bg-destructive/20 dark:text-destructive"
          >
            <hlm-avatar class="size-20">
                @if (profilePictureUrl()) {
                    <img hlmAvatarImage [src]="profilePictureUrl()!" [alt]="fullName()" />
                }
                <span hlmAvatarFallback [style.backgroundColor]="bgColor()" class="text-2xl font-medium">
                    {{ initials() }}
                </span>
            </hlm-avatar>
          </hlm-alert-dialog-media>
          <h2 hlmAlertDialogTitle>Permanently Delete Account?</h2>
          <p hlmAlertDialogDescription>
            This action is permanent and cannot be undone. To confirm, please type your <b>PASSWORD</b> in the field below.
          </p>
          <hlm-field class="mt-2">
            <label hlmFieldLabel for="password">Password</label>
            <input hlmInput type="password"/>
          </hlm-field>
        </hlm-alert-dialog-header>
        <hlm-alert-dialog-footer>
          <button 
            hlmAlertDialogAction 
            variant="destructive"
            (click)="onDelete()"
          >
            <ng-icon name="lucideTrash2" />
            Delete
          </button>
          <button 
            hlmAlertDialogCancel 
            (click)="onCancel()">
            Cancel
          </button>
        </hlm-alert-dialog-footer>
      </hlm-alert-dialog-content>
    </hlm-alert-dialog>`,
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class DeleteAccount {
  private readonly profileService = inject(ProfileService);
  private readonly alertService = inject(AlertService)
  private readonly authService = inject(AuthService);
  protected readonly router = inject(Router);
  protected state = signal<'open' | 'closed'>('open');

  readonly user = this.profileService.user;

  protected readonly profilePictureUrl = computed(() => this.user()?.profilePictureUrl ?? null);
  readonly fullName = computed(() => {
    const user = this.user();
    return user ? `${user.firstName} ${user.lastName}` : 'Guest';
  });
  readonly bgColor = computed(() => stringToColor(this.fullName()));
  readonly initials = computed(() => getInitials(this.fullName()));

  onStateChanged(state: 'open' | 'closed') {
    this.state.set(state);
    if (state === 'closed') {
        this.router.navigate(['/profile']);
    }
  }

  onCancel() {
    this.state.set('closed');
  }

  onDelete() {
    // call delete service, then navigate
    this.router.navigate(['/profile']);
  }
}
