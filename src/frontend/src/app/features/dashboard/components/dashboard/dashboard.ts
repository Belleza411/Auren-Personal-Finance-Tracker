import { ChangeDetectionStrategy, Component, computed, inject, resource } from '@angular/core';
import { DashboardService } from '../../services/dashboard-service';
import { TransactionService } from '../../../transactions/services/transaction.service';
import { firstValueFrom } from 'rxjs';
import { SummaryCard } from "../../../../shared/ui/summary-card/summary-card";
import { RouterLink } from "@angular/router";
import { CountUpDirective } from 'ngx-countup';
import { TransactionTable } from "../../../transactions/components/transaction-table/transaction-table";
import { signal } from '@angular/core';
import { TimePeriod } from '../../../transactions/models/transaction.model';
import { IncomeVsExpenseGraph } from "../income-vs-expense-graph/income-vs-expense-graph";
import { ExpenseBreakdownChart } from "../expense-breakdown-chart/expense-breakdown-chart";
import { TransactionStateService } from '../../../transactions/services/transaction-state.service';

@Component({
  selector: 'app-dashboard',
  imports: [
    SummaryCard,
    RouterLink,
    CountUpDirective,
    TransactionTable,
    IncomeVsExpenseGraph,
    ExpenseBreakdownChart
  ],
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class DashboardComponent {
  private dashboardSer = inject(DashboardService);
  private transactionSer = inject(TransactionService);
  private transactionStateSer = inject(TransactionStateService);

  selectedTimePeriod = signal<TimePeriod>(1);

  timePeriodOptions: string[] = ['All Time', 'This Month', 'Last Month', 'Last 3 Months', 'Last 6 Months', 'This Year'];
  
  totalBalance = computed(() => this.dashboardResources.value()?.totalBalance.totalBalance ?? { amount: 0, percentageChange: 0 });
  income = computed(() => this.dashboardResources.value()?.totalBalance.income ?? { amount: 0, percentageChange: 0 });
  expense = computed(() => this.dashboardResources.value()?.totalBalance.expense ?? { amount: 0, percentageChange: 0 });
  avgDailySpending = computed(() => this.dashboardResources.value()?.avgDailySpending ?? 0);
  incomeVsExpenseData = computed(() => this.dashboardResources.value()?.incomeVsExpenseData ?? { labels: [], expenses: [], incomes: []})  
  recentTransactions = computed(() => this.dashboardResources.value()?.recentTransactions.items ?? []);
  expenseBreakdown = computed(() => this.dashboardResources.value()?.expenseBreakdown ?? { labels: [], data: [], percentage: [], backgroundColor: [], totalSpent: 0});
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
        expenseBreakdown
      ] = await Promise.all([
        firstValueFrom(this.dashboardSer.getDashboardSummary(period)),
        firstValueFrom(this.transactionSer.getAvgDailySpending(period)),
        firstValueFrom(this.transactionStateSer.getTransactions({}, 5, 1)),
        firstValueFrom(this.dashboardSer.getIncomeVsExpense(period)),
        firstValueFrom(this.dashboardSer.getExpenseBreakdown(period)),      ])

      return {
        totalBalance,
        avgDailySpending,
        recentTransactions,
        incomeVsExpenseData,
        expenseBreakdown
      }
    }
  });

  onTimePeriodChange(e: Event) {
    this.selectedTimePeriod.set(
      Number((e.target as HTMLSelectElement).value) + 1
    );
  }
}
