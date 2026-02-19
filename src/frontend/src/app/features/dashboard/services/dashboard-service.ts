import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { apiUrl } from '../../../../environments/environment';
import { Observable } from 'rxjs';
import { DashboardSummary, ExpenseBreakdown, IncomeVsExpenseResponse } from '../models/dashboard.model';
import { TimePeriod } from '../../transactions/models/transaction.model';
import { createHttpParams } from '../../../shared/utils/http-params.util';

@Injectable({
  providedIn: 'root',
})
export class DashboardService {
  private http = inject(HttpClient);
  private baseUrl = `${apiUrl}/api/dashboard`;

  getDashboardSummary(timePeriod?: TimePeriod): Observable<DashboardSummary> {
    const params = this.createTimePeriodParams(timePeriod);

    return this.http.get<DashboardSummary>(`${this.baseUrl}/summary`, { params });
  }

  getExpenseBreakdown(timePeriod?: TimePeriod): Observable<ExpenseBreakdown> {
    const params = this.createTimePeriodParams(timePeriod);

    return this.http.get<ExpenseBreakdown>(`${this.baseUrl}/expense-breakdown`, { params });
  }

  getIncomeVsExpense(timePeriod?: TimePeriod): Observable<IncomeVsExpenseResponse> {
    const params = this.createTimePeriodParams(timePeriod);

    return this.http.get<IncomeVsExpenseResponse>(`${this.baseUrl}/income-vs-expense`, { params });
  }

  private createTimePeriodParams(timePeriod?: TimePeriod) {
    const filters = timePeriod !== undefined ? { timePeriod: timePeriod.toString().replace(/\s+/g, '') } : {};
    return createHttpParams(filters);
  }
}
