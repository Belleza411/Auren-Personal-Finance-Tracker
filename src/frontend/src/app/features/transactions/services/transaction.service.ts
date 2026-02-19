import { inject, Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable} from 'rxjs';

import { BalanceSummary, NewTransaction, PagedResult, TimePeriod, Transaction, TransactionFilter } from '../models/transaction.model';
import { apiUrl } from '../../../../environments/environment';
import { createHttpParams } from '../../../shared/utils/http-params.util';

@Injectable({
  providedIn: 'root',
})
export class TransactionService {
	private readonly baseUrl = `${apiUrl}/api/transactions`;
	private http = inject(HttpClient);

	getAllTransactions(
		params: HttpParams
	) : Observable<PagedResult<Transaction>> {		
		return this.http.get<PagedResult<Transaction>>(this.baseUrl, { params });
	}

	getTransactionById(id: string): Observable<Transaction> {
		return this.http.get<Transaction>(`${this.baseUrl}/${id}`);
	}

	createTransaction(data: NewTransaction): Observable<Transaction> {
		return this.http.post<Transaction>(this.baseUrl, data);
	}

	updateTransaction(id: string, data: Partial<NewTransaction>): Observable<Transaction> {
		return this.http.put<Transaction>(`${this.baseUrl}/${id}`, data);
	}

	deleteTransaction(id: string): Observable<void> {
		return this.http.delete<void>(`${this.baseUrl}/${id}`);
	}

	getAvgDailySpending(timePeriod?: TimePeriod): Observable<number> {
		const filters = timePeriod !== undefined ? { timePeriod } : {};
		const params = createHttpParams(filters);

		return this.http.get<number>(`${this.baseUrl}/average-daily-spending`, { params });
	}

	getBalance(timePeriod?: TimePeriod): Observable<BalanceSummary> {
		const filters = timePeriod !== undefined ? { timePeriod } : {};
		const params = createHttpParams(filters);

		return this.http.get<BalanceSummary>(`${this.baseUrl}/balance`, { params });
	}
}