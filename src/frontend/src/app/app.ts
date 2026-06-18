import { Component, signal } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { AlertContainer } from "./shared/components/alert-container/alert-container";

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, AlertContainer],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App {
  protected readonly title = signal('Auren');
}
