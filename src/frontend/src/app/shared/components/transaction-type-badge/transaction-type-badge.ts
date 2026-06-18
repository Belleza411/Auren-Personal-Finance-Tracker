import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import { UpperCasePipe } from '@angular/common';
import { TransactionTypeColorPipe } from '../../../features/transactions/pipes/transaction-type-color.pipe';
import { TransactionType } from 'src/app/features/transactions/models/transaction.model';

@Component({
  selector: 'app-transaction-type-badge',
  imports: [UpperCasePipe, TransactionTypeColorPipe],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <p class="text-[0.6rem] inline-flex items-center px-2 py-1 rounded-full font-medium"
       [class]="type() | transactionTypeColor:true">
      {{ type() | uppercase }}
    </p>
  `,
})
export class TransactionTypeBadge {
  type = input.required<TransactionType>();
}