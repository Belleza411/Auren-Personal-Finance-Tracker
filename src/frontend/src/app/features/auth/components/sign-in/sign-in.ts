import { Component, inject, signal } from '@angular/core';
import { form, required, email, submit, minLength, Field } from '@angular/forms/signals'
import { AuthService } from '../../service/auth-service';
import { Router, RouterLink } from '@angular/router';

import { Login } from '../../models/user.model';

@Component({
  selector: 'app-sign-in',
  imports: [Field, RouterLink],
  templateUrl: './sign-in.html',
  styleUrl: './sign-in.css',
})
export class SignInFormComponent {
  private readonly authSer = inject(AuthService);
  private router = inject(Router);

  isLoading = signal(false);
  error = signal<string | null>(null);

  loginModel = signal<Login>({
    email: '',
    password: ''
  });

  loginForm = form(this.loginModel, schema => {
    required(schema.email, { message: "Email is required "}),
    email(schema.email, { message: "Enter a valid email"}),

    required(schema.password, { message: "Password is required"}),
    minLength(schema.password, 8, { message: "Password must be at least 8 characters"})
  });
  
  onSubmit(event: Event) {
    event.preventDefault();

    submit(this.loginForm, async () => {
      this.isLoading.set(true);
      this.error.set(null);

      const credentials = this.loginModel()
      
      this.authSer.login(credentials)
        .subscribe({
          next: (response) => {
            console.log(response);
            this.router.navigate(['/dashboard']);
            this.isLoading.set(false);
          },
          error: err => {
            console.error("Login failed: ", err);
            this.error.set('Login failed. Please try again.');
            this.isLoading.set(false);
          }
        })
    })
  }
}
