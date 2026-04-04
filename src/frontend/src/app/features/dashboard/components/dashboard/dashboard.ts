import { ChangeDetectionStrategy, Component, computed, inject } from '@angular/core';
import { combineLatest, debounceTime, shareReplay, startWith, Subject, switchMap } from 'rxjs';
import { SummaryCard } from "../../../../shared/ui/summary-card/summary-card";
import { RouterLink } from "@angular/router";
import { CountUpDirective } from 'ngx-countup';
import { TransactionTable } from "../../../transactions/components/transaction-table/transaction-table";
import { signal } from '@angular/core';
import { TimePeriod } from '../../../transactions/models/transaction.model';
import { IncomeVsExpenseGraph } from "../income-vs-expense-graph/income-vs-expense-graph";
import { ExpenseBreakdownChart } from "../expense-breakdown-chart/expense-breakdown-chart";
import { toObservable, toSignal } from '@angular/core/rxjs-interop';
import { DashboardStateService } from '../../services/dashboard-state.service';
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
  private dashboardStateSer = inject(DashboardStateService);
  private transactionStateSer = inject(TransactionStateService);

  selectedTimePeriod = signal<TimePeriod>(1);

  timePeriodOptions: string[] = ['All Time', 'This Month', 'Last Month', 'Last 3 Months', 'Last 6 Months', 'This Year'];
  
  options = {
    duration: 1.2,
    separator: ',',
    prefix: '$',
    decimalPlaces: 2
  };

  private timePeriod$ = toObservable(computed(() => this.selectedTimePeriod()));
  private reload$ = new Subject<void>();

  private dashboardData$ = combineLatest([
    this.timePeriod$,
    this.reload$.pipe(startWith(null))
  ]).pipe(
    debounceTime(300),
    switchMap(([timePeriod]) => this.dashboardStateSer.getDashboardData(timePeriod)),
    shareReplay(1)
  )

  dashboardData = toSignal(this.dashboardData$, { initialValue: null });
  transactionData = toSignal(this.transactionStateSer.getTransactions({}, 5, 1), { initialValue: null });
  dashboardSummary = computed(() => this.dashboardData()?.summary ?? {
    totalBalance: { amount: 0, percentageChange: 0 },
    income: { amount: 0, percentageChange: 0 },
    expense: { amount: 0, percentageChange: 0 }
  });
  incomeVsExpenseData = computed(() => this.dashboardData()?.incomeVsExpense ?? {
    labels: [],
    incomes: [],
    expenses: []
  });
  expenseBreakdownData = computed(() => this.dashboardData()?.expenseBreakdown ?? {
    labels: [],
    data: [],
    percentage: [],
    backgroundColor: [],
    totalSpent: 0
  });
  avgDailySpending = computed(() => this.dashboardData()?.avgDailySpending ?? 0);
  recentTransactions = computed(() => this.transactionData()?.items ?? []);
  isLoading = computed(() => this.dashboardData() === null);

  onTimePeriodChange(e: Event) {
    this.selectedTimePeriod.set(
      Number((e.target as HTMLSelectElement).value) + 1
    );
  }
}
