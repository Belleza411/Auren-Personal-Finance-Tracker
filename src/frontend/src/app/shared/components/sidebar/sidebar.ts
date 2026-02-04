import { ChangeDetectionStrategy, Component, DestroyRef, inject, signal } from '@angular/core';
import { Router, RouterLink, RouterLinkActive } from "@angular/router";
import { AuthService } from '../../../features/auth/service/auth-service';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { SIDEBAR_MENU } from './sidebar-menu';
import { ProfileComponent } from "../../../features/profile/profile/profile";

@Component({
  selector: 'app-sidebar',
  imports: [RouterLink, RouterLinkActive, ProfileComponent],
  templateUrl: './sidebar.html',
  styleUrl: './sidebar.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class Sidebar {
  private readonly authSer = inject(AuthService);
  private destroyRef = inject(DestroyRef);
  private readonly router = inject(Router);

  readonly sidebarItems = SIDEBAR_MENU;

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
}
