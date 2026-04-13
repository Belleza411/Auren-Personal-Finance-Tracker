import { inject, Injectable } from '@angular/core';
import { CacheStateService } from '../../../core/services/cache-state.service';
import { DashboardData } from '../models/dashboard.model';
import { forkJoin, Observable } from 'rxjs';
import { DashboardService } from './dashboard.service';
import { TimePeriod, TimePeriodLabel } from '../../../core/models/time-period.enum';
import { createHttpParams } from '../../../shared/utils/http-params.util';

@Injectable({
  providedIn: 'root',
})
export class DashboardStateService extends CacheStateService<DashboardData, any> {
  private dashboardService = inject(DashboardService);
  protected override ttl: number = 120_00;
  protected override initialKey: string = "auren:dashboard";
  
  getDashboardData(timePeriod?: TimePeriod): Observable<DashboardData> {
    const filters = timePeriod !== undefined ? { timePeriod: TimePeriodLabel[timePeriod] }: {};
    
    const key = this.generateCacheKey(filters);
    return this.getFromCache(key, () => {
      const params = createHttpParams(filters);
      return forkJoin({
        summary: this.dashboardService.getDashboardSummary(params),
        incomeVsExpense: this.dashboardService.getIncomeVsExpense(params),
        expenseBreakdown: this.dashboardService.getExpenseBreakdown(params)
      });
    });
  } 
}
