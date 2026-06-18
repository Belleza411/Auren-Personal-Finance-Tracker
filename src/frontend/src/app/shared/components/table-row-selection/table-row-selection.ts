import { ChangeDetectionStrategy, Component } from '@angular/core';
import { injectFlexRenderContext, CellContext } from '@tanstack/angular-table';
import { Transaction } from 'src/app/features/transactions/models/transaction.model';
import { HlmCheckbox } from 'src/app/libs/ui/checkbox/src/lib/hlm-checkbox';

@Component({
  selector: 'app-table-row-selection',
  imports: [HlmCheckbox],
  template: `
    <hlm-checkbox type="checkbox"
      [checked]="context.row.getIsSelected()"
      (change)="context.row.toggleSelected()" />
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class TableRowSelection {
  protected readonly context = injectFlexRenderContext<CellContext<Transaction, unknown>>();
}
