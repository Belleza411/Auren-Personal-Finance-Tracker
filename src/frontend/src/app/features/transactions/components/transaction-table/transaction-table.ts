import { Component, computed, signal } from '@angular/core';
import { PaymentTypeMap, Transaction, TransactionType, TransactionTypeMap } from '../../models/transaction.model';
import { Category } from '../../../categories/models/categories.model';

@Component({
  selector: 'app-transaction-table',
  imports: [],
  templateUrl: './transaction-table.html',
  styleUrl: './transaction-table.css',
})
export class TransactionTable {
  transactions = signal<Transaction[]>([
    {
      transactionId: '1',
      userId: '1',
      categoryId: '1',
      transactionType: 1,
      name: 'Freelance Payment',
      amount: 2000.00,
      paymentType: 3,
      transactionDate: "June 1, 2025",
      createdAt: "June 1, 2025"
    },
    {
      transactionId: '1',
      userId: '1',
      categoryId: '1',
      transactionType: 1,
      name: 'Freelance Payment',
      amount: 2000.00,
      paymentType: 3,
      transactionDate: "June 1, 2025",
      createdAt: "June 1, 2025"
    },
    {
      transactionId: '1',
      userId: '1',
      categoryId: '1',
      transactionType: 1,
      name: 'Freelance Payment',
      amount: 2000.00,
      paymentType: 3,
      transactionDate: "June 1, 2025",
      createdAt: "June 1, 2025"
    },
    {
      transactionId: '1',
      userId: '1',
      categoryId: '1',
      transactionType: 1,
      name: 'Freelance Payment',
      amount: 2000.00,
      paymentType: 3,
      transactionDate: "June 1, 2025",
      createdAt: "June 1, 2025"
    },
    {
      transactionId: '1',
      userId: '1',
      categoryId: '1',
      transactionType: 1,
      name: 'Freelance Payment',
      amount: 2000.00,
      paymentType: 3,
      transactionDate: "June 1, 2025",
      createdAt: "June 1, 2025"
    },
    {
      transactionId: '1',
      userId: '1',
      categoryId: '1',
      transactionType: 1,
      name: 'Freelance Payment',
      amount: 2000.00,
      paymentType: 3,
      transactionDate: "June 1, 2025",
      createdAt: "June 1, 2025"
    },
    {
      transactionId: '1',
      userId: '1',
      categoryId: '1',
      transactionType: 1,
      name: 'Freelance Payment',
      amount: 2000.00,
      paymentType: 3,
      transactionDate: "June 1, 2025",
      createdAt: "June 1, 2025"
    }
  ]);

  categories =signal<Category[]>([
    {
      categoryId: '1',
      userId: '1',
      name: 'Salary',
      transactionType: 1,
      createdAt: "June 1, 2025"
    }
  ])

  protected TransactionTypeMap = TransactionTypeMap;
  protected PaymentTypeMap = PaymentTypeMap;
  protected categoryMap = computed(() => {
    return new Map(
      this.categories().map(c => [c.categoryId, c.name])
    );
  });

}
