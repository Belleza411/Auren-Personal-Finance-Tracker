import { ChangeDetectionStrategy, Component, computed, effect, inject, resource } from '@angular/core';
import { DashboardService } from '../../services/dashboard-service';
import { TransactionService } from '../../../transactions/services/transaction.service';
import { firstValueFrom } from 'rxjs';
import { SummaryCard } from "../../../../shared/components/summary-card/summary-card";
import { RouterLink } from "@angular/router";
import { GoalService } from '../../../goals/services/goal.service';
import { CountUpDirective } from 'ngx-countup';
import { TransactionTable } from "../../../transactions/components/transaction-table/transaction-table";
import { signal } from '@angular/core';
import { TimePeriod } from '../../../transactions/models/transaction.model';
import { IncomeVsExpenseGraph } from "../income-vs-expense-graph/income-vs-expense-graph";
import { ExpenseBreakdownChart } from "../expense-breakdown-chart/expense-breakdown-chart";
import { GoalComponent } from "../../../goals/components/goal/goal";
import { generateBgColorByEmoji } from '../../../goals/utils/generateBgColorByEmoji';
import { TransactionStateService } from '../../../transactions/services/transaction-state.service';
import { GoalWithBgColor } from '../../../goals/models/goals.model';

@Component({
  selector: 'app-dashboard',
  imports: [SummaryCard, RouterLink, CountUpDirective, TransactionTable, IncomeVsExpenseGraph, ExpenseBreakdownChart, GoalComponent],
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class DashboardComponent {
  private dashboardSer = inject(DashboardService);
  private transactionSer = inject(TransactionService);
  private transactionStateSer = inject(TransactionStateService);
  private goalSer = inject(GoalService);

  selectedTimePeriod = signal<TimePeriod>(1);

  timePeriodOptions: string[] = ['All Time', 'This Month', 'Last Month', 'Last 3 Months', 'Last 6 Months', 'This Year'];
  
  totalBalance = computed(() => this.dashboardResources.value()?.totalBalance.totalBalance ?? { amount: 0, percentageChange: 0 });
  income = computed(() => this.dashboardResources.value()?.totalBalance.income ?? { amount: 0, percentageChange: 0 });
  expense = computed(() => this.dashboardResources.value()?.totalBalance.expense ?? { amount: 0, percentageChange: 0 });
  avgDailySpending = computed(() => this.dashboardResources.value()?.avgDailySpending ?? 0);
  incomeVsExpenseData = computed(() => this.dashboardResources.value()?.incomeVsExpenseData ?? { labels: [], expenses: [], incomes: []})  
  recentTransactions = computed(() => this.dashboardResources.value()?.recentTransactions.items ?? []);
  currentGoals = computed(() => this.dashboardResources.value()?.recentGoals.items ?? []);
  currentGoalsWithBgColor = computed<GoalWithBgColor[]>(() => {
    return this.currentGoals().map(g => ({
      ...g,
      bgColor: generateBgColorByEmoji(g.emoji)
    }))
  })
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
        expenseBreakdown,
        recentGoals
      ] = await Promise.all([
        firstValueFrom(this.dashboardSer.getDashboardSummary(period)),
        firstValueFrom(this.transactionSer.getAvgDailySpending(period)),
        firstValueFrom(this.transactionStateSer.getTransactions({}, 5, 1)),
        firstValueFrom(this.dashboardSer.getIncomeVsExpense(period)),
        firstValueFrom(this.dashboardSer.getExpenseBreakdown(period)),
        firstValueFrom(this.goalSer.getAllGoals({}, 3, 1))
      ])

      return {
        totalBalance,
        avgDailySpending,
        recentTransactions,
        incomeVsExpenseData,
        expenseBreakdown,
        recentGoals
      }
    }
  });

  onTimePeriodChange(e: Event) {
    this.selectedTimePeriod.set(
      Number((e.target as HTMLSelectElement).value) + 1
    );
  }
}
