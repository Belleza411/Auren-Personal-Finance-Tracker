import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { apiUrl } from '../../../../environments/environment';
import { Observable } from 'rxjs';
import { DashboardSummary, ExpenseCategoryChart } from '../models/dashboard.model';
import { TimePeriod } from '../../transactions/models/transaction.model';
import { createHttpParams } from '../../../shared/utils/http-params.util';

@Injectable({
  providedIn: 'root',
})
export class DashboardService {
  private http = inject(HttpClient);
  private baseUrl = `${apiUrl}/dashboard`;

  getDashboardSummary(timePeriod?: TimePeriod): Observable<DashboardSummary> {
    const filters = timePeriod !== undefined ? { timePeriod } : {};
    const params = createHttpParams(filters);

    return this.http.get<DashboardSummary>(`${this.baseUrl}/summary`, { params });
  }

  getExpenseCategoryChart(): Observable<ExpenseCategoryChart[]> {
    return this.http.get<ExpenseCategoryChart[]>(`${this.baseUrl}/categories-expense`);
  }
}
