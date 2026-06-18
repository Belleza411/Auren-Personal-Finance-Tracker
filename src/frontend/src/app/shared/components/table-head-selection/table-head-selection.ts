import { ChangeDetectionStrategy, Component } from '@angular/core';
import { injectFlexRenderContext, HeaderContext } from '@tanstack/angular-table';
import { Transaction } from 'src/app/features/transactions/models/transaction.model';
import { HlmCheckbox } from 'src/app/libs/ui/checkbox/src/lib/hlm-checkbox';

@Component({
  selector: 'app-table-head-selection',
  imports: [HlmCheckbox],
  template: `
    <hlm-checkbox type="checkbox"
      [checked]="context.table.getIsAllRowsSelected()"
      [indeterminate]="context.table.getIsSomeRowsSelected()"
      (change)="context.table.toggleAllRowsSelected()" />
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class TableHeadSelection {
  protected readonly context = injectFlexRenderContext<HeaderContext<Transaction, unknown>>();
}
