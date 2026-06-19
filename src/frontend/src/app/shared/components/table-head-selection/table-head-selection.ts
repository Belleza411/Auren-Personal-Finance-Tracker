import { ChangeDetectionStrategy, ChangeDetectorRef, Component, inject, input, OnChanges } from '@angular/core';
import { HeaderContext, injectFlexRenderContext, Table } from '@tanstack/angular-table';
import { Transaction } from 'src/app/features/transactions/models/transaction.model';
import { HlmCheckbox } from 'src/app/libs/ui/checkbox/src/lib/hlm-checkbox';

@Component({
  selector: 'app-table-head-selection',
  imports: [HlmCheckbox],
  template: `
    <hlm-checkbox type="checkbox"
      [checked]="ctx.table.getIsSomeRowsSelected() ? 'indeterminate' : ctx.table.getIsAllRowsSelected()"
      (checkedChange)="ctx.table.toggleAllRowsSelected($event === true)"/>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class TableHeadSelection {
  protected ctx = injectFlexRenderContext<HeaderContext<Transaction, unknown>>();
}
