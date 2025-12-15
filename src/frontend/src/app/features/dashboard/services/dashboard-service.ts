import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { apiUrl } from '../../../../environments/environment';
import { Observable } from 'rxjs';
import { DashboardSummary, ExpenseCategoryChart } from '../models/dashboard.model';

@Injectable({
  providedIn: 'root',
})
export class DashboardService {
  private http = inject(HttpClient);
  private baseUrl = `${apiUrl}/dashboard`;

  getDashboardSummary(): Observable<DashboardSummary> {
    return this.http.get<DashboardSummary>(`${this.baseUrl}/summary`);
  }

  getExpenseCategoryChart(): Observable<ExpenseCategoryChart[]> {
    return this.http.get<ExpenseCategoryChart[]>(`${this.baseUrl}/categories-expense`);
  }
}
