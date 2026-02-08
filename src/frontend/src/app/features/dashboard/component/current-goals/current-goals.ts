import { Component, input } from '@angular/core';
import { Goal } from '../../../goals/models/goals.model';

@Component({
  selector: 'app-current-goals',
  imports: [],
  templateUrl: './current-goals.html',
  styleUrl: './current-goals.css',
})
export class CurrentGoals {
  readonly currentGoals = input.required<Goal[]>();
}
