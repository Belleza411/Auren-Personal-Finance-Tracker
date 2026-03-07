import { ChangeDetectionStrategy, Component } from '@angular/core';
import { RouterLink, RouterLinkActive } from "@angular/router";
import { ProfileComponent } from "../../../features/profile/profile/profile";
import { NAVBAR_MENU } from './navbar-menu';

@Component({
  selector: 'app-navbar',
  imports: [RouterLink, RouterLinkActive, ProfileComponent],
  templateUrl: './navbar.html',
  styleUrl: './navbar.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class Navbar {
  readonly sidebarItems = NAVBAR_MENU;
}
