import { inject, Injectable } from '@angular/core';
import { CacheStateService } from '../../../core/services/cache-state.service';
import { DashboardData } from '../models/dashboard.model';
import { forkJoin, Observable } from 'rxjs';
import { TimePeriod } from '../../transactions/models/transaction.model';
import { createTimePeriodParams } from '../../../shared/utils/createTimePeriodParams.util';
import { DashboardService } from './dashboard.service';
import { TransactionService } from '../../transactions/services/transaction.service';

@Injectable({
  providedIn: 'root',
})
export class DashboardStateService extends CacheStateService<DashboardData, any> {
  private dashboardService = inject(DashboardService);
  private transactionService = inject(TransactionService);
  protected override ttl: number = 120_00;
  protected override initialKey: string = "auren:dashboard";
  
  getDashboardData(timePeriod?: TimePeriod): Observable<DashboardData> {
    const params = createTimePeriodParams(timePeriod);
    const key = this.generateCacheKey(params);
    return this.getFromCache(key, () => {
      return forkJoin({
        summary: this.dashboardService.getDashboardSummary(params),
        incomeVsExpense: this.dashboardService.getIncomeVsExpense(params),
        expenseBreakdown: this.dashboardService.getExpenseBreakdown(params),
        avgDailySpending: this.transactionService.getAvgDailySpending(params)
      });
    });
  } 
}
