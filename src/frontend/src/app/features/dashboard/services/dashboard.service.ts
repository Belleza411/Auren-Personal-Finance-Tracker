import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { apiUrl } from '../../../../environments/environment';
import { Observable } from 'rxjs';
import { DashboardSummary, ExpenseBreakdown, IncomeVsExpenseResponse } from '../models/dashboard.model';

@Injectable({
  providedIn: 'root',
})
export class DashboardService {
  private http = inject(HttpClient);
  private baseUrl = `${apiUrl}/api/dashboard`;

  getDashboardSummary(params: HttpParams): Observable<DashboardSummary> {
    return this.http.get<DashboardSummary>(`${this.baseUrl}/summary`, { params });
  }

  getExpenseBreakdown(params: HttpParams): Observable<ExpenseBreakdown> {
    return this.http.get<ExpenseBreakdown>(`${this.baseUrl}/expense-breakdown`, { params });
  }

  getIncomeVsExpense(params: HttpParams): Observable<IncomeVsExpenseResponse> {
    return this.http.get<IncomeVsExpenseResponse>(`${this.baseUrl}/income-vs-expense`, { params });
  }
}