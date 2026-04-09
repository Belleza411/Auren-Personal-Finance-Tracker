import { ChangeDetectionStrategy, Component, inject, OnInit, signal } from '@angular/core';
import { RouterLink, RouterLinkActive } from "@angular/router";
import { ProfileComponent } from "../../../features/profile/profile/profile";
import { NAVBAR_MENU } from './navbar-menu';
import { BreakpointObserver } from '@angular/cdk/layout';

@Component({
  selector: 'app-navbar',
  imports: [RouterLink, RouterLinkActive, ProfileComponent],
  templateUrl: './navbar.html',
  styleUrl: './navbar.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class Navbar implements OnInit {
  private breakpointObserver = inject(BreakpointObserver);
  readonly sidebarItems = NAVBAR_MENU;
  isMobile = signal(false);
  isMenuOpen = signal(false);
  isClosing = signal(false);
  
  ngOnInit(): void {
    this.breakpointObserver
      .observe(['(max-width: 640px)'])
      .subscribe(result => {
        this.isMobile.set(result.matches);
        if(!this.isMobile) this.isMenuOpen.set(false);
      })
  }

  toggleMenu() {
    if(this.isMenuOpen() && !this.isClosing()) {
      this.closeMenu();
    } else {
      this.isMenuOpen.set(true);
    }
  }

  closeMenu() {
    this.isClosing.set(true);
    setTimeout(() => {
      this.isMenuOpen.set(false);
      this.isClosing.set(false);
    }, 200); 
  }
}