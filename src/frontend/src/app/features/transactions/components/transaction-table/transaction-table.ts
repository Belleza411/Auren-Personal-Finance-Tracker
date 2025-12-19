import { Component, computed, input, signal } from '@angular/core';
import { PaymentTypeMap, Transaction, TransactionType, TransactionTypeMap } from '../../models/transaction.model';
import { Category } from '../../../categories/models/categories.model';

@Component({
  selector: 'app-transaction-table',
  imports: [],
  templateUrl: './transaction-table.html',
  styleUrl: './transaction-table.css',
})
export class TransactionTable {
  transactions = input.required<Transaction[]>();
  categories = input.required<Category[]>();

  protected TransactionTypeMap = TransactionTypeMap;
  protected PaymentTypeMap = PaymentTypeMap;
  protected categoryMap = computed(() => {
    return new Map(
      this.categories().map(c => [c.categoryId, c.name])
    );
  });

}
