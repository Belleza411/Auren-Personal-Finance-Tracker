import { Component, DestroyRef, inject, OnInit, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { finalize, forkJoin } from 'rxjs';

import { TransactionService } from '../../services/transaction.service';
import { AvgDailySpending, Transaction } from '../../models/transaction.model';

@Component({
  selector: 'app-transaction',
  imports: [],
  templateUrl: './transactions.component.html',
  styleUrl: './transactions.component.css',
})
export class TransactionComponent implements OnInit {
  private transactionSer = inject(TransactionService);
  private destroyRef = inject(DestroyRef);

  transactions = signal<Transaction[]>([]);
  avgDailySpending = signal<AvgDailySpending | null>(null);
  isLoading = signal(false);
  error = signal<string | null>(null);
  
  ngOnInit(): void {
    this.loadData();
  }  

  private loadData() {
    this.isLoading.set(true);
    this.error.set(null);

    forkJoin({
      transactions: this.transactionSer.getAllTransactions({}),
      avgDailySpending: this.transactionSer.getAvgDailySpending(),
    })
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        finalize(() => this.isLoading.set(false))
      )
      .subscribe({
        next: ({ transactions, avgDailySpending }) => {
          this.transactions.set(transactions);
          this.avgDailySpending.set(avgDailySpending);
        },
        error: err => {
          console.error("Failed to load data: ", err);
          this.error.set("Failed to load data. Please try again.")
        }
      })
  }

  deleteTransaction(id: string) {
    this.transactionSer.deleteTransaction(id)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.transactions.update(list => list.filter(t => t.transactionId !== id)),
          this.reloadAvgDailySpending();
        },
        error: err => {
          console.error('Failed to delete transaction:', err);
          this.error.set('Failed to delete transaction. Please try again.');
        }
      })
  }

  private reloadAvgDailySpending() {
    this.transactionSer.getAvgDailySpending()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: data => this.avgDailySpending.set(data),
        error: err => console.error('Failed to reload average spending:', err)
      })
  }

  retry() {
    this.loadData();
  }
}
