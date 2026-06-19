import { ChangeDetectionStrategy, Component } from '@angular/core';
import { CellContext, injectFlexRenderContext } from '@tanstack/angular-table';
import { Transaction } from 'src/app/features/transactions/models/transaction.model';
import { HlmCheckbox } from 'src/app/libs/ui/checkbox/src/lib/hlm-checkbox';

@Component({
  selector: 'app-table-row-selection',
  imports: [HlmCheckbox],
  template: `
    <hlm-checkbox type="checkbox"
      [checked]="ctx.row.getIsSelected()"
      [disabled]="!ctx.row.getCanSelect()"
      (checkedChange)="ctx.row.toggleSelected($event === true)" />
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class TableRowSelection {
  protected ctx = injectFlexRenderContext<CellContext<Transaction, unknown>>();
}
