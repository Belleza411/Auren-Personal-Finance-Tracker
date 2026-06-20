import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import { NgIcon, provideIcons } from '@ng-icons/core';
import { lucideArrowUp, lucideArrowDown  } from '@ng-icons/lucide';
import { HeaderContext, injectFlexRenderContext } from '@tanstack/angular-table';
import { Transaction } from 'src/app/features/transactions/models/transaction.model';

@Component({
  selector: 'app-table-head-sort-button',
  imports: [NgIcon],
  providers: [provideIcons({ lucideArrowUp, lucideArrowDown })],
  template: `
    <button 
      class="flex items-center justify-center gap-1"
      hlmBtn 
      size="icon" 
      variant="ghost"
      (click)="context.column.getToggleSortingHandler()?.($event)"
    > 
      {{ header() }}
      <ng-icon [name]="context.column.getIsSorted() === 'asc' 
        ? 'lucideArrowUp' 
        : context.column.getIsSorted() === 'desc' 
        ? 'lucideArrowDown'
        : 'lucideArrowDown'" 
        size="1rem"
      />
    </button>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class TableHeadSortButton {
  header = input.required<string>();
  protected readonly context = injectFlexRenderContext<HeaderContext<Transaction, unknown>>();
}
