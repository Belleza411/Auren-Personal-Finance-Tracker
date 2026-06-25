import { ChangeDetectionStrategy, Component, DestroyRef, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { form, required, minLength, validate, submit, FormField, disabled } from '@angular/forms/signals';
import { ChangePasswordRequest } from 'src/app/core/auth/models/user.model';
import { AuthService } from 'src/app/core/auth/service/auth.service';
import { AlertService } from 'src/app/core/services/alert.service';
import { createFieldErrors } from 'src/app/shared/utils/form-errors.util';
import { HlmCardImports } from './../../../../libs/ui/card/src';
import { HlmInputImports } from '@spartan-ng/helm/input';
import { HlmFieldImports } from '@spartan-ng/helm/field';
import { HlmButtonImports } from '@spartan-ng/helm/button';
import { HlmSeparatorImports } from '@spartan-ng/helm/separator';
import { HlmSpinnerImports } from './../../../../libs/ui/spinner/src/index';
import { lucidePencil } from '@ng-icons/lucide';
import { NgIcon, provideIcons } from '@ng-icons/core';


@Component({
  selector: 'app-change-password',
  imports: [
    HlmButtonImports,
    HlmCardImports,
    HlmInputImports,
    HlmFieldImports,
    HlmSeparatorImports,
    HlmSpinnerImports,
    FormField,
    NgIcon
  ],
  providers: [provideIcons({ lucidePencil })],
  templateUrl: './change-password.html',
  styleUrl: './change-password.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ChangePassword {
  private authService = inject(AuthService);
  private alertService = inject(AlertService)
  private destroyRef = inject(DestroyRef);
  isLoading = signal(false);
  isEditing = signal(false);

  protected passwordModel = signal<ChangePasswordRequest>({
    currentPassword: '',
    newPassword: '',
    confirmPassword: '',
  });

  protected passwordForm = form(this.passwordModel, schema => {
    required(schema.currentPassword, { message: 'Password is required' });
    minLength(schema.currentPassword, 8, { message: 'Password must be at least 8 characters' });
    required(schema.newPassword, { message: 'New password is required' });
    minLength(schema.newPassword, 8, { message: 'New password must be at least 8 characters' });
    required(schema.confirmPassword, { message: 'Confirm password is required' });

    disabled(schema.currentPassword, { when: () => !this.isEditing()})
    disabled(schema.newPassword, { when: () => !this.isEditing()})
    disabled(schema.confirmPassword, { when: () => !this.isEditing()})

    validate(schema.newPassword, ({ value, valueOf }) => {
      const newPassword = value();
      const confirmPassword = valueOf(schema.confirmPassword);

      if(newPassword !== confirmPassword) {
        return {
          kind: 'passwordMismatch',
          message: "Passwords do not match"
        }
      }

      return null;
    })
  })

  onChangePassword(event: Event) {
    event.preventDefault();
    submit(this.passwordForm, async () => {
        this.isLoading.set(true);

        try {
          const request = this.passwordModel();
          this.authService.changePassword(request)
            .pipe(takeUntilDestroyed(this.destroyRef))
            .subscribe({
              next: () => {
                this.isLoading.set(false);
                this.alertService.success('Password updated successfully', 'Password Updated')
              },
              error: e => {
                this.isLoading.set(false)
                this.alertService.error('Failed to update password', e.error?.messages ?? 'Failed to update password')
              }
            })
        } finally {
            this.isLoading.set(false);
            this.passwordModel.set({ currentPassword: '', newPassword: '', confirmPassword: '' });
        }
    });
  }

  onEdit() {
    this.isEditing.set(true);
  }

  onCancel() {
    this.isEditing.set(false);
  }

  protected readonly fieldErrors = createFieldErrors({
    currentPassword: () => this.passwordForm.currentPassword(),
    newPassword: () => this.passwordForm.newPassword(),
    confirmPassword: () => this.passwordForm.confirmPassword(),
  })
}
