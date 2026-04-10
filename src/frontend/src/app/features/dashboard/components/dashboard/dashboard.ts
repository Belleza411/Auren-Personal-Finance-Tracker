import { ChangeDetectionStrategy, Component, computed, inject } from '@angular/core';
import { combineLatest, shareReplay, startWith, Subject, switchMap } from 'rxjs';
import { SummaryCard } from "../../../../shared/ui/summary-card/summary-card";
import { RouterLink } from "@angular/router";
import { CountUpDirective } from 'ngx-countup';
import { TransactionTable } from "../../../transactions/components/transaction-table/transaction-table";
import { signal } from '@angular/core';
import { IncomeVsExpenseGraph } from "../income-vs-expense-graph/income-vs-expense-graph";
import { ExpenseBreakdownChart } from "../expense-breakdown-chart/expense-breakdown-chart";
import { toObservable, toSignal } from '@angular/core/rxjs-interop';
import { DashboardStateService } from '../../services/dashboard-state.service';
import { TransactionStateService } from '../../../transactions/services/transaction-state.service';
import { TimePeriod, TimePeriodLabel } from '../../../../core/models/time-period.enum';
import { TIME_PERIOD_OPTIONS } from '../../../../shared/constants/type-options';
import { DASHBOARD_SUMMARY_INITIAL_DATA, EXPENSE_BREAKDOWN_INITIAL_DATA, INCOME_VS_EXPENSE_INITIAL_DATA } from '../constants/dashboard-data';
import { PercentageBgColorPipe } from '../../pipes/percentage-bg-color.pipe';

@Component({
  selector: 'app-dashboard',
  imports: [
    SummaryCard,
    RouterLink,
    CountUpDirective,
    TransactionTable,
    IncomeVsExpenseGraph,
    ExpenseBreakdownChart,
    PercentageBgColorPipe
  ],
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class DashboardComponent {
  private dashboardStateSer = inject(DashboardStateService);
  private transactionStateSer = inject(TransactionStateService);

  selectedTimePeriod = signal<TimePeriod>(2);
  TimePeriodLabel = TimePeriodLabel

  timePeriodOptions = TIME_PERIOD_OPTIONS
  
  options = {
    duration: 1.2,
    separator: ',',
    prefix: '$',
    decimalPlaces: 2
  };

  private timePeriod$ = toObservable(this.selectedTimePeriod);
  private reload$ = new Subject<void>();

  private dashboardData$ = combineLatest([
    this.timePeriod$,
    this.reload$.pipe(startWith(null))
  ]).pipe(
    switchMap(([timePeriod]) => {
      return this.dashboardStateSer.getDashboardData(timePeriod)
    }),
    shareReplay(1)
  )

  dashboardData = toSignal(this.dashboardData$, { initialValue: null });
  transactionData = toSignal(this.transactionStateSer.getTransactions({}, 5, 1), { initialValue: null });
  dashboardSummary = computed(() => this.dashboardData()?.summary ?? DASHBOARD_SUMMARY_INITIAL_DATA);
  incomeVsExpenseData = computed(() => this.dashboardData()?.incomeVsExpense ?? INCOME_VS_EXPENSE_INITIAL_DATA);
  expenseBreakdownData = computed(() => this.dashboardData()?.expenseBreakdown ?? EXPENSE_BREAKDOWN_INITIAL_DATA);
  avgDailySpending = computed(() => this.dashboardData()?.avgDailySpending ?? 0);
  recentTransactions = computed(() => this.transactionData()?.items ?? []);
  isLoading = computed(() => this.dashboardData() === null);

  onTimePeriodChange(e: Event) {
    this.selectedTimePeriod.set(
      Number((e.target as HTMLSelectElement).value) as TimePeriod
    );
  }

  getArrowIconByPercentageChange(percentageChange: number): string {
    if(percentageChange > 0) return 'arrow_upward';
    if(percentageChange < 0) return 'arrow_downward';
    return '';
  }
}
