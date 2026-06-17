import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { SummaryCard } from "../../../../shared/ui/summary-card/summary-card";
import { Router, RouterLink } from "@angular/router";
import { CountUpDirective } from 'ngx-countup';
import { TransactionTable } from "../../../transactions/components/transaction-table/transaction-table";
import { IncomeVsExpenseGraph } from "../income-vs-expense-graph/income-vs-expense-graph";
import { ExpenseBreakdownChart } from "../expense-breakdown-chart/expense-breakdown-chart";
import { rxResource, toSignal } from '@angular/core/rxjs-interop';
import { DashboardStateService } from '../../services/dashboard-state.service';
import { TransactionStateService } from '../../../transactions/services/transaction-state.service';
import { TimePeriod, TimePeriodLabel } from '../../../../core/models/time-period.enum';
import { TIME_PERIOD_OPTIONS } from '../../../../shared/constants/type-options';
import { DASHBOARD_SUMMARY_INITIAL_DATA, EXPENSE_BREAKDOWN_INITIAL_DATA, INCOME_VS_EXPENSE_INITIAL_DATA } from '../constants/dashboard-data';
import { PercentageBgColorPipe } from '../../pipes/percentage-bg-color.pipe';
import { TimePeriodService } from '../../../../core/services/time-period.service';
import { ArrowIconByPercentageChangePipe } from '../../pipes/arrow-icon-by-percentage.pipe';

@Component({
  selector: 'app-dashboard',
  imports: [
    RouterLink,
    SummaryCard,
    CountUpDirective,
    TransactionTable,
    IncomeVsExpenseGraph,
    ExpenseBreakdownChart,
    PercentageBgColorPipe,
    ArrowIconByPercentageChangePipe
  ],
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class DashboardComponent {
  private dashboardStateSer = inject(DashboardStateService);
  private transactionStateSer = inject(TransactionStateService);
  private timePeriodService = inject(TimePeriodService);
  private router = inject(Router);

  TimePeriodLabel = TimePeriodLabel
  timePeriodOptions = TIME_PERIOD_OPTIONS
  
  options = {
    duration: 1.2,
    separator: ',',
    prefix: '$',
    decimalPlaces: 2
  };

  private timePeriod$ = this.timePeriodService.selectedTimePeriod$;
  private selectedTimePeriodSignal = toSignal(this.timePeriod$, { initialValue: 2 as TimePeriod });
  private reloadTrigger = signal(0)

  dashboardResource = rxResource({
    params: () => ({
      timePeriod: this.selectedTimePeriodSignal(),
      reload: this.reloadTrigger()
    }),
    stream: ({ params }) => this.dashboardStateSer.getDashboardData(params.timePeriod)
  });

  recentTransactionsResource = rxResource({
    stream: () => this.transactionStateSer.getTransactions({}, 5, 1)
  });

  summaryCards = computed(() => [
    { label: 'Total Balance',      icon: 'attach_money',  ...this.dashboardSummary().totalBalance          },
    { label: 'Income',             icon: 'trending_up',   ...this.dashboardSummary().income                },
    { label: 'Expense',            icon: 'trending_down', ...this.dashboardSummary().expense               },
    { label: 'Avg Daily Spending', icon: 'attach_money',  ...this.dashboardSummary().averageDailySpending  },
  ]);

  dashboardSummary = computed(() => this.dashboardResource.value()?.summary ?? DASHBOARD_SUMMARY_INITIAL_DATA);
  incomeVsExpenseData = computed(() => this.dashboardResource.value()?.incomeVsExpense ?? INCOME_VS_EXPENSE_INITIAL_DATA);
  expenseBreakdownData = computed(() => this.dashboardResource.value()?.expenseBreakdown ?? EXPENSE_BREAKDOWN_INITIAL_DATA);
  recentTransactions = computed(() => this.recentTransactionsResource.value()?.items ?? []);
  isLoading = computed(() => this.dashboardResource.isLoading());

  selectedTimePeriod = this.selectedTimePeriodSignal;

  onTimePeriodChange(e: Event) {
    this.timePeriodService.setTimePeriod(
      Number((e.target as HTMLSelectElement).value) as TimePeriod
    );
  }

  onAddTransaction(): void {
      this.router.navigate(['/transactions', 'create']);
  }
}
