import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import { TransactionType } from 'src/app/features/transactions/models/transaction.model';
import { TransactionAmountSignPipe } from 'src/app/features/transactions/pipes/transaction-amount-sign.pipe';
import { TransactionTypeColorPipe } from 'src/app/features/transactions/pipes/transaction-type-color.pipe';

@Component({
  selector: 'app-transaction-amount-badge',
  imports: [TransactionAmountSignPipe, TransactionTypeColorPipe],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <p class="text-[0.8rem] inline-flex items-center px-2 py-1 rounded-full font-medium"
      [class]="type() | transactionTypeColor:false"
    >
      {{ type() | transactionAmountSign }}{{ formatted }}
    </p>
  `,
})
export class TransactionAmountBadge {
  type = input.required<TransactionType>();
  amount = input.required<number>();

  protected get formatted(): string {
    return new Intl.NumberFormat('en-US', {
      style: 'currency',
      currency: 'USD',
    }).format(this.amount());
  }
}