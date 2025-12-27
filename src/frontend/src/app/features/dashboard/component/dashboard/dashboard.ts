import { Component, DestroyRef, inject, OnInit, signal } from '@angular/core';
import { DashboardService } from '../../services/dashboard-service';
import { TransactionService } from '../../../transactions/services/transaction.service';
import { DashboardSummary, ExpenseCategoryChart } from '../../models/dashboard.model';
import { Transaction } from '../../../transactions/models/transaction.model';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { finalize, forkJoin } from 'rxjs';
import { SummaryCard } from "../../../../shared/components/summary-card/summary-card";
import { RouterLink } from "@angular/router";

@Component({
  selector: 'app-dashboard',
  imports: [SummaryCard, RouterLink],
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.css',
})
export class DashboardComponent implements OnInit {
  private dashboardSer = inject(DashboardService);
  private transactionSer = inject(TransactionService);
  private destroyRef = inject(DestroyRef);

  error = signal<string | null>(null);
  isLoading = signal(false);
  dashboardSummaries = signal<DashboardSummary | null>(null);
  expenseCategoriesChart = signal<ExpenseCategoryChart[] | null>(null);
  recentTransactions = signal<Transaction[]>([]);

  ngOnInit(): void {
    this.loadData();
  }

  private loadData() {
    this.isLoading.set(true);
    this.error.set(null);

    forkJoin({
      dashboardSummaries: this.dashboardSer.getDashboardSummary(),
      expenseCategoriesChart: this.dashboardSer.getExpenseCategoryChart(),
      transactions: this.transactionSer.getAllTransactions()
    })
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        finalize(() => this.isLoading.set(false))
      )
      .subscribe({
        next: ({  dashboardSummaries, expenseCategoriesChart, transactions }) => {
          this.dashboardSummaries.set(dashboardSummaries),
          this.expenseCategoriesChart.set(expenseCategoriesChart),
          this.recentTransactions.set(transactions.items)
        },
        error: err => {
          console.error("Failed to load data: ", err);
          this.error.set("Failed to load data. Please try again.")
        }
      })
  }
}
