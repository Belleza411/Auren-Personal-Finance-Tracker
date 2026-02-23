import { ChangeDetectionStrategy, Component, computed, input, output, signal } from '@angular/core';
import { CurrencyPipe, UpperCasePipe } from '@angular/common';

import { Transaction } from '../../models/transaction.model';
import { COMPACT_TRANSACTION_COLUMNS, FULL_TRANSACTION_COLUMNS } from '../../models/transaction-column.model';
import { TransactionAmountSignPipe } from '../../pipes/transaction-amount-sign.pipe';
import { TransactionTypeColorPipe } from '../../pipes/transaction-type-color.pipe';
import { AddWhitespacePipe } from '../../pipes/add-whitespace.pipe';

@Component({
  selector: 'app-transaction-table',
  imports: [
    CurrencyPipe,
    UpperCasePipe,
    TransactionAmountSignPipe,
    TransactionTypeColorPipe,
    AddWhitespacePipe
  ],
  templateUrl: './transaction-table.html',
  styleUrl: './transaction-table.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: {
    '[class.compact]': "variant() === 'compact'"
  }
})
export class TransactionTable {
  transactions = input.required<Transaction[]>();
  isLoading = input<boolean>();
  variant = input<'full' | 'compact'>('full');

  columns = computed(() =>
    this.variant() === 'compact'
      ? COMPACT_TRANSACTION_COLUMNS
      : FULL_TRANSACTION_COLUMNS
  );
  
  pageNumberChange = output<number>();
  pageSizeChange = output<number>();

  pageNumber = signal(1);
  pageSize = signal(10);
  totalCount = input<number>();

  pageSizeOptions: number[] = [10, 20, 30, 40, 50];

  openModalId = signal<string | null>(null);

  toggleModalId(id: string) {
    this.openModalId.update(current =>
      current === id ? null : id
    );
  }
  
  delete = output<string>();
  edit = output<string>(); 

  onDelete(id: string) {
    this.delete.emit(id);
  }

  onEdit(id: string) {
    this.edit.emit(id);
  }
}  
