import { ChangeDetectionStrategy, Component, computed, DestroyRef, effect, inject, OnInit, signal } from '@angular/core';
import { HlmButtonImports } from '@spartan-ng/helm/button';
 import { HlmAvatarImports } from '@spartan-ng/helm/avatar';
import { HlmSeparatorImports } from '@spartan-ng/helm/separator';
import { UserDto, UserResponse } from '../../models/profile.model';
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
import { HlmSelectImports } from '@spartan-ng/helm/select';
import { email, form, submit, FormField, disabled, minLength, required, validate } from '@angular/forms/signals';
import { AlertService } from 'src/app/core/services/alert.service';
import { createFieldErrors } from 'src/app/shared/utils/form-errors.util';

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
    HlmSelectImports,
    NgIcon,
    FormField
],
  providers: [provideIcons({ lucideTrash, lucideUpload, lucidePencil })],
  templateUrl: './profile.html',
  styleUrl: './profile.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class Profile {
  private profileService = inject(ProfileService);
  private alertService = inject(AlertService)
  private destroyRef = inject(DestroyRef);

  currencyOptions = [
    { value: 'USD', label: 'USD — US Dollar' },
    { value: 'EUR', label: 'EUR — Euro' },
    { value: 'GBP', label: 'GBP — British Pound' },
    { value: 'NZD', label: 'NZD — New Zealand Dollar' },
    { value: 'AUD', label: 'AUD — Australian Dollar' },
  ];

  readonly user = this.profileService.user;

  isEditing = signal(false)
  isLoading = signal(false)
  isPasswordLoading = signal(false);
  selectedFile = signal<File | null>(null);
  profilePreview = signal<string | null>(null);

  protected profileModel = signal<UserDto>({
    email: '',
    firstName: '',
    lastName: '',
    profileImageUploadRequest: null,
    currency: ''
  })

  protected passwordModel = signal({
    currentPassword: '',
    newPassword: '',
    confirmPassword: '',
  });

  protected profileForm = form(this.profileModel, schema => {
    email(schema.email, { message: 'Enter a valid email'})

    disabled(schema.firstName, { when: () => !this.isEditing()})
    disabled(schema.lastName, { when: () => !this.isEditing()})
    disabled(schema.email, { when: () => !this.isEditing()})
    disabled(schema.currency, { when: () => !this.isEditing()})
  })

  protected passwordForm = form(this.passwordModel, schema => {
    required(schema.currentPassword, { message: 'Password is required' });
    minLength(schema.currentPassword, 8, { message: 'Password must be at least 8 characters' });
    required(schema.confirmPassword, { message: 'Confirm password is required' });

    validate(schema.confirmPassword, ({ value, valueOf }) => {
      const confirm = value();
      const password = valueOf(schema.currentPassword);

      if(confirm !== password) {
        return {
          kind: 'passwordMismatch',
          message: "Passwords do not match"
        }
      }

      return null;
    })
  })

  protected readonly profilePictureUrl = computed(() => this.user()?.profilePictureUrl ?? null);
  protected pendingProfileImage =   signal<File | null>(null);
  protected previewUrl = computed(() => {
    const file = this.pendingProfileImage();
    return file ? URL.createObjectURL(file) : null;
  });

  readonly email = computed(() => this.user()?.email ?? "guest@gmail.com")
  readonly fullName = computed(() => {
    const user = this.user();
    return user ? `${user.firstName} ${user.lastName}` : 'Guest';
  });

  readonly bgColor = computed(() => stringToColor(this.fullName()));
  readonly initials = computed(() => getInitials(this.fullName()));

  constructor() {
    effect(() => {
        const user = this.user();
        if (!user) return;
        this.profileModel.set({
          email: user.email ?? '',
          firstName: user.firstName ?? '',
          lastName: user.lastName ?? '',
          profileImageUploadRequest: null,
          currency: "USD"
        });
    });
  }

  onSubmit(event: Event) {
    event.preventDefault();
    if (!this.isEditing()) return;
    submit(this.profileForm, async () => {
        this.isLoading.set(true);

        const profile = this.profileModel();
        const formData = new FormData();

        formData.append('Email', profile.email ?? '');
        formData.append('FirstName', profile.firstName ?? '');
        formData.append('LastName', profile.lastName ?? '');
        formData.append('Currency', profile.currency ?? '');

        const file = profile.profileImageUploadRequest;
        if (file && file?.file) {
          formData.append('profilePictureUrl.file', file.file);
          formData.append('profilePictureUrl.name', file.name ?? '');
          formData.append('profilePictureUrl.description', file.description ?? '');
        }

        this.profileService.updateUser(formData)
          .pipe(takeUntilDestroyed(this.destroyRef))
          .subscribe({
              next: () => {
                this.pendingProfileImage.set(null);
                this.isLoading.set(false);
                this.isEditing.set(false);
                this.alertService.success('Profile Information Updated', 'Updated Successfully');
              },
              error: e => {
                this.isLoading.set(false);
                this.alertService.error('Failed to update profile', e.error?.messages ?? 'Please try again.');
              }
          });
    });
  }

  onEdit() {
    this.isEditing.set(true);
  }

  onCancel() {
    this.isEditing.set(false);
  }

  onChangePicture(event: Event) {
    const file = (event.target as HTMLInputElement).files?.[0];
    if (!file) return;
    this.pendingProfileImage.set(file);
    this.profileModel.update(m => ({ 
      ...m, 
      profileImageUploadRequest: {
        file: file,
        name: m.profileImageUploadRequest?.name ?? '',
        description: m.profileImageUploadRequest?.description ?? ''
      }
    }));
  }

  onDeletePicture() {
    this.pendingProfileImage.set(null);
    this.profileModel.update(m => ({ 
      ...m,
      profileImageUploadRequest: null
    }));
  }

  onDeleteAccount() {
    // show confirmation dialog before deleting
  }

  onChangePassword(event: Event) {
    event.preventDefault();
    submit(this.passwordForm, async () => {
        this.isPasswordLoading.set(true);
        try {
            // call your password change service here
        } finally {
            this.isPasswordLoading.set(false);
            this.passwordModel.set({ currentPassword: '', newPassword: '', confirmPassword: '' });
        }
    });
  }

  currencyToString = (value: string): string =>
    this.currencyOptions.find(c => c.value === value)?.label ?? '';

  onCurrencyChange(value: string | null | undefined) {
    if (!value) return;
    this.profileModel.update(m => ({ ...m, currency: value }));
  }
}
