import { Component, inject, OnInit, signal } from '@angular/core';
import { TransactionService } from '../../services/transaction.service';
import { Transaction } from '../../models/transaction.model';

@Component({
  selector: 'app-transaction',
  imports: [],
  templateUrl: './transactions.html',
  styleUrl: './transactions.css',
})
export class TransactionComponent implements OnInit {
  private transactionService = inject(TransactionService);
  
  transactions = signal<Transaction[]>([]);
  isLoading = signal(false);
  
  ngOnInit(): void {
    this.loadTransactions();
  }

  loadTransactions() {
    this.isLoading.set(true);
    this.transactionService.getAllTransactions({})
      .subscribe({
        next: data => this.transactions.set(data),
        error: err => console.error(err),
        complete: () => this.isLoading.set(false)
      })
  }

  deleteTransaction(id: string) {
    this.transactionService.deleteTransaction(id)
      .subscribe({
        next: () => this.transactions.update(transaction =>
          transaction.filter(t => t.transactionId !== id)
        ),
        error: err => console.error(err)
      })
  }
}
