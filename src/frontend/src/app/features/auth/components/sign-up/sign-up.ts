import { ChangeDetectionStrategy, Component, computed, DestroyRef, inject, signal, } from '@angular/core';
import { Router, RouterLink } from "@angular/router";
import { AuthService } from '../../service/auth-service';
import { Register } from '../../models/user.model';
import { email, FieldState, form, minLength, required, submit, validate, FormField } from '@angular/forms/signals';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';

@Component({
  selector: 'app-sign-up',
  imports: [RouterLink, FormField],
  templateUrl: './sign-up.html',
  styleUrl: './sign-up.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class SignUpFormComponent {
  private readonly authSer = inject(AuthService);
  private router = inject(Router);
  private destroyRef = inject(DestroyRef);

  isLoading = signal(false);
  error = signal<string | null>(null);
  showPassword = signal(false);
  showConfirmPassword = signal(false);
  selectedFile = signal<File | null>(null);
  profilePreview = signal<string | null>(null);

  protected registerModel = signal<Register>({
    email: '',
    password: '',
    confirmPassword: '',
    firstName: '',
    lastName: '',
    profileImage: {
      file: null,
      name: '',
      description: ''
    }
  });

  protected registerForm = form(this.registerModel, schema => {
    required(schema.email, { message: 'Email is required' });
    email(schema.email, { message: 'Enter a valid email' });
    required(schema.password, { message: 'Password is required' });
    minLength(schema.password, 8, { message: 'Password must be at least 8 characters' });
    required(schema.confirmPassword, { message: 'Confirm password is required' });
    required(schema.firstName, { message: 'First name is required' });
    required(schema.lastName, { message: 'Last name is required' });

    validate(schema.confirmPassword, ({ value, valueOf }) => {
      const confirm = value();
      const password = valueOf(schema.password);

      if(confirm !== password) {
        return {
          kind: 'passwordMismatch',
          message: "Passwords do not match"
        }
      }

      return null;
    })
  });

  onFileSelect(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files.length > 0) {
      const file = input.files[0];
      this.selectedFile.set(file);

      this.registerModel.update(model => ({
        ...model,
        profileImage: {
          ...model.profileImage,
          file: file,
          name: model.profileImage?.name ?? '',
          description: model.profileImage?.description ?? ''
        }
      }));

      const reader = new FileReader();
      reader.onload = () => this.profilePreview.set(reader.result as string);
      reader.readAsDataURL(file); 
    }
  }

  onSubmit(event: Event) {
    event.preventDefault();

    submit(this.registerForm, async () => {
      this.isLoading.set(true);
      this.error.set(null);

      const credentials = this.registerModel();    

      this.authSer.register(credentials)
        .pipe(takeUntilDestroyed(this.destroyRef))
        .subscribe({
          next: (response) => {
            console.log(response);
            this.profilePreview.set(null);
            this.selectedFile.set(null);
            this.router.navigate(['/auth/sign-in']);
            this.isLoading.set(false)
          },
          error: err => {
            console.error('Registration failed: ', err);
            this.error.set(err?.error?.message || 'Registration failed. Please try again.');
            this.isLoading.set(false);
          }
        })
    })
  }

  protected fieldErrors = {
    email: this.createErrorSignal(() => this.registerForm.email()),
    password: this.createErrorSignal(() => this.registerForm.password()),
    confirmPassword: this.createErrorSignal(() => this.registerForm.confirmPassword()),
    firstName: this.createErrorSignal(() => this.registerForm.firstName()),
    lastName: this.createErrorSignal(() => this.registerForm.lastName())
  };

  private createErrorSignal(field: () => FieldState<string>) {
    return computed(() => this.setShowError(field()));
  };

  private setShowError(field: FieldState<string>) {
    return field.invalid() && field.touched();
  };

  protected togglePassword(): void {
    this.showPassword.update(v => !v);
  }

  protected toggleConfirmPassword(): void {
    this.showConfirmPassword.update(v => !v);
  }
}
