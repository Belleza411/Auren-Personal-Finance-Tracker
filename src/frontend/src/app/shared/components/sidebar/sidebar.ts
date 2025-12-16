import { Component, DestroyRef, inject, signal } from '@angular/core';
import { Router, RouterLink } from "@angular/router";
import { AuthService } from '../../../features/auth/service/auth-service';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';

@Component({
  selector: 'app-sidebar',
  imports: [RouterLink],
  templateUrl: './sidebar.html',
  styleUrl: './sidebar.css',
})
export class Sidebar {
  private readonly authSer = inject(AuthService);
  private destroyRef = inject(DestroyRef);
  private readonly router = inject(Router);

  error = signal<string | null>(null);

  logout() {
    this.authSer.logout()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => this.router.navigate(['/auth/sign-in']),
        error: err => {
          console.error("Something went wrong ", err);
          this.error.set("Something went wrong. ");
          this.router.navigate(['/auth/sign-in']);
        }
      })
  }
}
