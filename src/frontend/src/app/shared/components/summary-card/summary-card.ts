import { CurrencyPipe } from '@angular/common';
import { Component, computed, input } from '@angular/core';

@Component({
  selector: 'app-summary-card',
  imports: [CurrencyPipe],
  templateUrl: './summary-card.html',
  styleUrl: './summary-card.css',
})
export class SummaryCard {
  title = input.required<string>();
  icon = input.required<string>();
  amount = input.required<number>();
  percentage = input.required<number>();

  isPositive = computed(() => this.percentage() >= 0);
}
