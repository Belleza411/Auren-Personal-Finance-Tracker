import { Component } from '@angular/core';
import { Sidebar } from "../../../shared/components/sidebar/sidebar";

@Component({
  selector: 'app-main-layout',
  imports: [Sidebar],
  templateUrl: './main-layout.html',
  styleUrl: './main-layout.css',
})
export class MainLayoutComponent {

}
