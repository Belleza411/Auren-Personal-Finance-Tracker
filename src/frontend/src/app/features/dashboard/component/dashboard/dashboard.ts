import { ChangeDetectionStrategy, Component, computed, inject, resource } from '@angular/core';
import { DashboardService } from '../../services/dashboard-service';
import { TransactionService } from '../../../transactions/services/transaction.service';
import { firstValueFrom } from 'rxjs';
import { SummaryCard } from "../../../../shared/components/summary-card/summary-card";
import { RouterLink } from "@angular/router";
import { GoalService } from '../../../goals/services/goal.service';
import { CountUpDirective } from 'ngx-countup';
import { TransactionTable } from "../../../transactions/components/transaction-table/transaction-table";
import { signal } from '@angular/core';
import { Transaction } from '../../../transactions/models/transaction.model';
import { Category } from '../../../categories/models/categories.model';

@Component({
  selector: 'app-dashboard',
  imports: [SummaryCard, RouterLink, CountUpDirective, TransactionTable],
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class DashboardComponent {
  private dashboardSer = inject(DashboardService);
  private transactionSer = inject(TransactionService);
  private goalSer = inject(GoalService);

  protected readonly dummyCategories = signal<Category[]>([
        {
            categoryId: '1',
            userId: '1',
            name: 'Salary',
            transactionType: 1,
            createdAt: "June 1, 2025"
        },
        {
            categoryId: '2',
            userId: '1',
            name: 'Shopping',
            transactionType: 2,
            createdAt: "June 2, 2025"
        },
        {
            categoryId: '3',
            userId: '1',
            name: 'Health',
            transactionType: 2,
            createdAt: "June 10, 2025"
        }
  ]);

  protected dummyTransactions = signal<Transaction[]>([
        {
            transactionId: '1',
            userId: '1',
            categoryId: '1',
            category: this.dummyCategories()[0],
            transactionType: 1,
            name: 'Freelance Payment',
            amount: 2000.0,
            paymentType: 3,
            transactionDate: 'June 1, 2025',
            createdAt: 'June 1, 2025'
        },
        {
            transactionId: '2',
            userId: '1',
            categoryId: '2',
            category: this.dummyCategories()[1],
            transactionType: 2,
            name: 'Groceries',
            amount: 100.0,
            paymentType: 3,
            transactionDate: 'June 2, 2025',
            createdAt: 'June 2, 2025'
        },
        {
            transactionId: '3',
            userId: '1',
            categoryId: '3',
            category: this.dummyCategories()[2],
            transactionType: 2,
            name: 'Health Insurance',
            amount: 55.0,
            paymentType: 3,
            transactionDate: 'June 10, 2025',
            createdAt: 'June 10, 2025'
        },
        {
            transactionId: '1',
            userId: '1',
            categoryId: '1',
            category: this.dummyCategories()[0],
            transactionType: 1,
            name: 'Freelance Payment',
            amount: 2000.0,
            paymentType: 3,
            transactionDate: 'June 1, 2025',
            createdAt: 'June 1, 2025'
        },
        {
            transactionId: '2',
            userId: '1',
            categoryId: '2',
            category: this.dummyCategories()[1],
            transactionType: 2,
            name: 'Groceries',
            amount: 100.0,
            paymentType: 3,
            transactionDate: 'June 2, 2025',
            createdAt: 'June 2, 2025'
        },
        {
            transactionId: '3',
            userId: '1',
            categoryId: '3',
            category: this.dummyCategories()[2],
            transactionType: 2,
            name: 'Health Insurance',
            amount: 55.0,
            paymentType: 3,
            transactionDate: 'June 10, 2025',
            createdAt: 'June 10, 2025'
        }
  ]);

  selectedTimePeriod: string[] = ['All Time', 'This Month', 'Last Month', 'Last 3 Months', 'Last 6 Months', 'This Year'];
  
  totalBalance = computed(() => this.dashboardResources.value()?.totalBalance.totalBalance ?? { amount: 2500, percentageChange: 20 });
  income = computed(() => this.dashboardResources.value()?.totalBalance.income ?? { amount: 2000, percentageChange: 10 });
  expense = computed(() => this.dashboardResources.value()?.totalBalance.expense ?? { amount: 500, percentageChange: 2 });
  avgDailySpending = computed(() => this.dashboardResources.value()?.avgDailySpending ?? 50.11);
  recentTransactions = computed(() => this.dashboardResources.value()?.recentTransactions.items ?? this.dummyTransactions());
  currentGoals = computed(() => this.dashboardResources.value()?.recentGoals.items ?? [])
  expenseCategoriesChart = computed(() => this.dashboardResources.value()?.expenseCategoriesChart ?? []);
  isLoading = computed(() => this.dashboardResources.isLoading());

  options = {
    duration: 1.2,
    separator: ',',
    prefix: '$',
    decimalPlaces: 2
  };

  dashboardResources = resource({
    loader: async () => {
      const [
        totalBalance, 
        avgDailySpending,
        recentTransactions,
        expenseCategoriesChart,
        recentGoals
      ] = await Promise.all([
        firstValueFrom(this.dashboardSer.getDashboardSummary()),
        firstValueFrom(this.transactionSer.getAvgDailySpending()),
        firstValueFrom(this.transactionSer.getAllTransactions({}, 5, 1)),
        firstValueFrom(this.dashboardSer.getExpenseCategoryChart()),
        firstValueFrom(this.goalSer.getAllGoals({}, 3, 1))
      ])

      return {
        totalBalance,
        avgDailySpending,
        recentTransactions,
        expenseCategoriesChart,
        recentGoals
      }
    }
  })
}
