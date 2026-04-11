import { ChangeDetectionStrategy, Component, computed, inject, input, output, signal } from '@angular/core';
import { CurrencyPipe, UpperCasePipe } from '@angular/common';

import { Transaction } from '../../models/transaction.model';
import { COMPACT_TRANSACTION_COLUMNS, FULL_TRANSACTION_COLUMNS } from '../../models/transaction-column.model';
import { TransactionAmountSignPipe } from '../../pipes/transaction-amount-sign.pipe';
import { TransactionTypeColorPipe } from '../../pipes/transaction-type-color.pipe';
import { AddWhitespacePipe } from '../../pipes/add-whitespace.pipe';
import { NavigationEnd, Router } from '@angular/router';

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
  private router = inject(Router);

  transactions = input.required<Transaction[]>();
  isLoading = input<boolean>();
  variant = input<'full' | 'compact'>('full');

  columns = computed(() =>
    this.variant() === 'compact'
      ? COMPACT_TRANSACTION_COLUMNS
      : FULL_TRANSACTION_COLUMNS
  );

  constructor() {
    this.router.events.subscribe(e => {
      if(e instanceof NavigationEnd) {
        if(e.url.includes('edit')) {
          this.openModalId.set(null)
        }
      }
    })
  }
  
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
