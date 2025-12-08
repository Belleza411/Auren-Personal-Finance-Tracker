import { inject, Injectable, signal } from '@angular/core';
import { CreateTransaction, Transaction, TransactionFilter } from '../models/transaction.model';
import { apiUrl } from '../../../../environments/environment';
import { Observable} from 'rxjs';
import { HttpClient, HttpParams } from '@angular/common/http';

@Injectable({
  providedIn: 'root',
})
export class TransactionService {
	private readonly baseUrl = `${apiUrl}/api/transactions`;
	private http = inject(HttpClient);

	getAllTransactions(
		filters: Partial<TransactionFilter>,
		pageSize: number = 5,
		pageNumber: number = 1
	) : Observable<Transaction[]> {
		let params = new HttpParams()
		.set('pageNumber', pageNumber.toString())
		.set('pageSize', pageSize.toString());  
	
		Object.entries(filters).forEach(([key, value]) => {
		if (value !== undefined && value !== null) {
			params = params.set(key, value.toString());
		}
		});

		return this.http.get<Transaction[]>(this.baseUrl, { params });
	}

	getTransactionById(id: string): Observable<Transaction> {
		return this.http.get<Transaction>(`${this.baseUrl}/${id}`);
	}

	createTransaction(data: CreateTransaction): Observable<Transaction> {
		return this.http.post<Transaction>(this.baseUrl, data);
	}

	updateTransaction(id: string, data: Partial<CreateTransaction>): Observable<Transaction> {
		return this.http.put<Transaction>(`${this.baseUrl}/${id}`, data);
	}

	deleteTransaction(id: string): Observable<void> {
		return this.http.delete<void>(`${this.baseUrl}/${id}`);
	}
}
