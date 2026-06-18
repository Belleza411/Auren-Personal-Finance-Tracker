import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import { NgIcon, provideIcons } from '@ng-icons/core';
import { lucideMoveUp, lucideMoveDown  } from '@ng-icons/lucide';
import { HeaderContext, injectFlexRenderContext } from '@tanstack/angular-table';
import { Transaction } from 'src/app/features/transactions/models/transaction.model';

@Component({
  selector: 'app-table-head-sort-button',
  imports: [NgIcon],
  providers: [provideIcons({ lucideMoveUp, lucideMoveDown })],
  template: `
    <button 
      hlmBtn 
      size="icon" 
      variant="ghost"
      (click)="context.column.getToggleSortingHandler()?.($event)"
    >
      {{ header() }}
      <ng-icon [name]="context.column.getIsSorted() === 'asc' 
        ? 'lucideMoveUp' 
        : context.column.getIsSorted() === 'desc' 
        ? 'lucideMoveDown'
        : 'lucideMoveDown'" 
      />
    </button>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class TableHeadSortButton {
  header = input.required<string>();
  protected readonly context = injectFlexRenderContext<HeaderContext<Transaction, unknown>>();
}
