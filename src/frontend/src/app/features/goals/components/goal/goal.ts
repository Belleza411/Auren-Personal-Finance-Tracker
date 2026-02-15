import { Component, computed, input, output } from '@angular/core';
import { Goal, GoalWithBgColor } from '../../models/goals.model';
import { CurrencyPipe } from '@angular/common';

@Component({
  selector: 'app-goal',
  imports: [CurrencyPipe],
  templateUrl: './goal.html',
  styleUrl: './goal.css',
})
export class GoalComponent {
  goal = input.required<GoalWithBgColor>();
  variant = input<'full' | 'compact'>('full');

  isVariantFull = computed(() => this.variant() === 'full');

  goalStatusOptions: string[] = ['All Status', 'Completed', 'On Track', 'On Hold', 'Not Started', 'Behind Schedule', 'Cancelled'];

  delete = output<string>();
  edit = output<string>();
  addMoney = output<string>();
  status = output<number>();

  onDelete(id: string) {
    this.delete.emit(id);
  }

  onEdit(id: string) {
    this.edit.emit(id);
  }

  onAddMoney(id: string) {
    this.addMoney.emit(id);
  }

  onStatusChange(e: Event) {
    const value = Number((e.target as HTMLSelectElement).value);
    this.status.emit(value);
  }
}