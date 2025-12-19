import { Component, DestroyRef, inject, OnInit, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { finalize, forkJoin } from 'rxjs';
import { RouterLink } from "@angular/router";

import { TransactionService } from '../../services/transaction.service';
import { Transaction } from '../../models/transaction.model';
import { TransactionTable } from "../../components/transaction-table/transaction-table";
import { SummaryCard } from "../../../../shared/components/summary-card/summary-card";
import { CurrencyPipe } from '@angular/common';

@Component({
  selector: 'app-transaction',
  imports: [TransactionTable, RouterLink, SummaryCard, CurrencyPipe],
  templateUrl: './transactions.component.html',
  styleUrl: './transactions.component.css',
})
export class TransactionComponent implements OnInit {
    private transactionSer = inject(TransactionService);
    private destroyRef = inject(DestroyRef);

    transactions = signal<Transaction[]>([]);
    avgDailySpending = signal<number>(500);
    totalBalance = signal<number>(2500);
    income = signal<number>(2000);
    expense = signal<number>(500);
    isLoading = signal(false);
    error = signal<string | null>(null);
    
    ngOnInit(): void {
        this.loadData();
    }  

    private loadData() {
        this.isLoading.set(true);
        this.error.set(null);

        forkJoin({
            transactions: this.transactionSer.getAllTransactions(),
            avgDailySpending: this.transactionSer.getAvgDailySpending(),
            balance: this.transactionSer.getBalance()
        })
            .pipe(
                takeUntilDestroyed(this.destroyRef),
                finalize(() => this.isLoading.set(false))
            )
            .subscribe({
                next: ({ transactions, avgDailySpending, balance }) => {
                    this.transactions.set(transactions);
                    this.avgDailySpending.set(avgDailySpending);
                    this.totalBalance.set(balance.balance);
                    this.income.set(balance.income);
                    this.expense.set(balance.expense);
                },
                error: err => {
                    console.error("Failed to load data: ", err);
                    this.error.set("Failed to load data. Please try again.");
                }
        })
    }

    deleteTransaction(id: string) {
        this.transactionSer.deleteTransaction(id)
            .pipe(takeUntilDestroyed(this.destroyRef))
            .subscribe({
                next: () => {
                    this.transactions.update(list => list.filter(t => t.transactionId !== id)),
                    this.loadData()
                },
                error: err => {
                    console.error('Failed to delete transaction:', err);
                    this.error.set('Failed to delete transaction. Please try again.');
                }
            })
    }

    retry() {
        this.loadData();
    }
}
