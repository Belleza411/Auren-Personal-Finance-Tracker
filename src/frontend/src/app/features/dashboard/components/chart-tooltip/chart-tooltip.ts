import { CurrencyPipe } from '@angular/common';
import { Component, signal } from '@angular/core';

@Component({
  selector: 'app-chart-tooltip',
  imports: [CurrencyPipe],
  templateUrl: './chart-tooltip.html',
  styleUrl: './chart-tooltip.css',
})
export class ChartTooltip {
  visible = signal(false);
  x = signal(0);
  y = signal(0);
  date = signal<string>('');
  items = signal<{
    label: 'Incomes' | 'Expenses';
    value: number;
    color: string;
  }[]>([]);

  protected getValueColorByLabel(label: 'Incomes' | 'Expenses'): string {
    return label === 'Incomes' ? 'text-green-500' : 'text-red-600'
  }

  protected getArithmeticByLabel(label: 'Incomes' | 'Expenses'): string {
    return label === 'Incomes' ? '+' : '-';
  }
}
