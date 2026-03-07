import { ChangeDetectionStrategy, Component } from '@angular/core';
import { Navbar } from "../../../shared/ui/navbar/navbar.component";
import { RouterOutlet } from '@angular/router';

@Component({
  selector: 'app-main-layout',
  imports: [RouterOutlet, Navbar],
  templateUrl: './main-layout.html',
  styleUrl: './main-layout.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class MainLayout {

}
