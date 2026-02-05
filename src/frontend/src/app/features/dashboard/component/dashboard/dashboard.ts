import { Component, computed, inject, resource } from '@angular/core';
import { DashboardService } from '../../services/dashboard-service';
import { TransactionService } from '../../../transactions/services/transaction.service';
import { firstValueFrom } from 'rxjs';
import { SummaryCard } from "../../../../shared/components/summary-card/summary-card";
import { RouterLink } from "@angular/router";
import { GoalService } from '../../../goals/services/goal.service';
import { CountUpDirective } from 'ngx-countup';


@Component({
  selector: 'app-dashboard',
  imports: [SummaryCard, RouterLink, CountUpDirective],
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.css',
})
export class DashboardComponent {
  private dashboardSer = inject(DashboardService);
  private transactionSer = inject(TransactionService);
  private goalSer = inject(GoalService);

  totalBalance = computed(() => this.dashboardResources.value()?.totalBalance.totalBalance ?? { amount: 2500, percentageChange: 20 });
  income = computed(() => this.dashboardResources.value()?.totalBalance.income ?? { amount: 2000, percentageChange: 10 });
  expense = computed(() => this.dashboardResources.value()?.totalBalance.expense ?? { amount: 500, percentageChange: 2 });
  avgDailySpending = computed(() => this.dashboardResources.value()?.avgDailySpending ?? 50.11);
  recentTransactions = computed(() => this.dashboardResources.value()?.recentTransactions.items ?? []);
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
