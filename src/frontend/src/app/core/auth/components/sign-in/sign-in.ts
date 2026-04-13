import { ChangeDetectionStrategy, Component, DestroyRef, inject, signal } from '@angular/core';
import { form, required, email, submit, minLength, FormField } from '@angular/forms/signals'
import { AuthService } from '../../service/auth.service';
import { Router, RouterLink } from '@angular/router';

import { Login } from '../../models/user.model';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ToastrService } from '../../../services/toastr.service';
import { createFieldErrors } from '../../../../shared/utils/form-errors.util';

@Component({
  selector: 'app-sign-in',
  imports: [FormField, RouterLink],
  templateUrl: './sign-in.html',
  styleUrls: ['./sign-in.css', '../../styles/auth.css'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class SignInFormComponent {
  private readonly authSer = inject(AuthService);
  private toastr = inject(ToastrService);
  private router = inject(Router);
  private destroyRef = inject(DestroyRef);

  isLoading = signal(false);
  showPassword = signal(false);

  protected loginModel = signal<Login>({
    email: '',
    password: ''
  });

  protected loginForm = form(this.loginModel, schema => {
    required(schema.email, { message: "Email is required "}),
    email(schema.email, { message: "Enter a valid email"}),

    required(schema.password, { message: "Password is required"}),
    minLength(schema.password, 8, { message: "Password must be at least 8 characters"})
  });
  
  onSubmit(event: Event) {
    event.preventDefault();

    submit(this.loginForm, async () => {
      this.isLoading.set(true);

      const credentials = this.loginModel()
      
      this.authSer.login(credentials)
        .pipe(takeUntilDestroyed(this.destroyRef))
        .subscribe({
          next: () => {
            this.router.navigate(['/dashboard']);
            this.isLoading.set(false);
          },
          error: () => {
            this.toastr.showError('Invalid email or password', 'Login Failed');
            this.isLoading.set(false);
          }
        })
    })
  }

  protected readonly fieldErrors = createFieldErrors({
    email: () => this.loginForm.email(),
    password: () => this.loginForm.password()
  })

  protected togglePassword(): void {
    this.showPassword.update(v => !v);
  }
}
