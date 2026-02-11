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
import { TimePeriod, Transaction } from '../../../transactions/models/transaction.model';
import { Category } from '../../../categories/models/categories.model';
import { Goal } from '../../../goals/models/goals.model';
import { CurrentGoals } from "../current-goals/current-goals";
import { IncomeVsExpenseGraph } from "../income-vs-expense-graph/income-vs-expense-graph";
import { IncomeVsExpenseResponse } from '../../models/dashboard.model';
import { TimePeriodMap } from '../../../../shared/utils/enum-mapper.util';

@Component({
  selector: 'app-dashboard',
  imports: [SummaryCard, RouterLink, CountUpDirective, TransactionTable, CurrentGoals, IncomeVsExpenseGraph],
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

  dummyGoals = signal<Goal[]>([
    {
      goalId: 'goal-001',
      userId: 'user-123',
      name: 'Buy a New Laptop',
      description: 'Save money to buy a high-performance laptop for development and studies.',
      spent: 450,
      budget: 1500,
      goalStatus: 2,
      completionPercentage: 30,
      timeRemaining: '3 months',
      createdAt: 'January 5, 2025',
      targetDate: 'April 30, 2025',
    },
    {
      goalId: 'goal-002',
      userId: 'user-123',
      name: 'Emergency Savings Fund',
      description: 'Build an emergency fund for unexpected expenses.',
      spent: 1200,
      budget: 3000,
      goalStatus: 2,
      completionPercentage: 40,
      timeRemaining: '6 months',
      createdAt: 'December 1, 2024',
      targetDate: 'August 1, 2025',
    },
    {
      goalId: 'goal-003',
      userId: 'user-123',
      name: 'Vacation Trip',
      description: 'Save for a short holiday trip with friends.',
      spent: null,
      budget: 2000,
      goalStatus: 4,
      completionPercentage: null,
      timeRemaining: '9 months',
      createdAt: 'January 20, 2025',
      targetDate: 'October 15, 2025',
    }
  ])

  dummyChartData = signal<IncomeVsExpenseResponse>(
    {
      labels: ['Jan', 'Jan 1', 'Jan 2', 'Jan 10', 'Feb', 'Feb 14', 'Mar', 'Mar 20', 'Mar 21'],
      incomes: [0, 200, 150, 0, 300, 0, 400, 120, 200],
      expenses: [0, 50, 80, 0, 200, 0, 90, 60, 50]
    }
  )

  TimePeriodMap = TimePeriodMap;

  selectedTimePeriod = signal<TimePeriod>(1);

  timePeriodOptions: string[] = ['All Time', 'This Month', 'Last Month', 'Last 3 Months', 'Last 6 Months', 'This Year'];
  
  totalBalance = computed(() => this.dashboardResources.value()?.totalBalance.totalBalance ?? { amount: 2500, percentageChange: 20 });
  income = computed(() => this.dashboardResources.value()?.totalBalance.income ?? { amount: 2000, percentageChange: 10 });
  expense = computed(() => this.dashboardResources.value()?.totalBalance.expense ?? { amount: 500, percentageChange: 2 });
  avgDailySpending = computed(() => this.dashboardResources.value()?.avgDailySpending ?? 50.11);
  incomeVsExpenseData = computed(() => this.dashboardResources.value()?.incomeVsExpenseData ?? this.dummyChartData())  
  recentTransactions = computed(() => this.dashboardResources.value()?.recentTransactions.items ?? this.dummyTransactions());
  currentGoals = computed(() => this.dashboardResources.value()?.recentGoals.items ?? this.dummyGoals())
  expenseCategoriesChart = computed(() => this.dashboardResources.value()?.expenseCategoriesChart ?? []);
  isLoading = computed(() => this.dashboardResources.isLoading());

  options = {
    duration: 1.2,
    separator: ',',
    prefix: '$',
    decimalPlaces: 2
  };

  dashboardResources = resource({
    params: () => ({
      timePeriod: this.selectedTimePeriod()
    }),
    loader: async ({ params }) => {
      const period = params.timePeriod;
      
      const [
        totalBalance, 
        avgDailySpending,
        recentTransactions,
        incomeVsExpenseData,
        expenseCategoriesChart,
        recentGoals
      ] = await Promise.all([
        firstValueFrom(this.dashboardSer.getDashboardSummary(period)),
        firstValueFrom(this.transactionSer.getAvgDailySpending(period)),
        firstValueFrom(this.transactionSer.getAllTransactions({}, 5, 1)),
        firstValueFrom(this.dashboardSer.getIncomeVsExpense(period)),
        firstValueFrom(this.dashboardSer.getExpenseCategoryChart()),
        firstValueFrom(this.goalSer.getAllGoals({}, 3, 1))
      ])

      return {
        totalBalance,
        avgDailySpending,
        recentTransactions,
        incomeVsExpenseData,
        expenseCategoriesChart,
        recentGoals
      }
    }
  });

  onRangeChange(e: Event) {
      this.selectedTimePeriod.set(
        Number((e.target as HTMLSelectElement).value) + 1
      );
    }
}
