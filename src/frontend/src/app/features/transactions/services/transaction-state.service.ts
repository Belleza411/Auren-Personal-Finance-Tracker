import { inject, Injectable } from '@angular/core';
import { TransactionService } from './transaction.service';
import { Observable, tap } from 'rxjs';
import { NewTransaction, PagedResult, Transaction, TransactionFilter } from '../models/transaction.model';
import { createHttpParams } from '../../../shared/utils/http-params.util';
import { CacheStateService } from '../../../core/services/cache-state.service';

@Injectable({
  providedIn: 'root',
})
export class TransactionStateService extends CacheStateService<PagedResult<Transaction>, TransactionFilter> {
  transactionService = inject(TransactionService);
  protected override ttl: number = 120_000;
  protected override initialKey: string = "auren:transactions";

  getTransactions(
    filters?: Partial<TransactionFilter>,
    pageSize?: number,
    pageNumber?: number
  ): Observable<PagedResult<Transaction>> {    
    const params = createHttpParams(filters, pageSize, pageNumber);
    const key = this.generateCacheKey(filters, pageSize, pageNumber)
    return this.getFromCache(key, () => this.transactionService.getAllTransactions(params))
  }

  deleteTransaction(id: string): Observable<void> {
    return this.transactionService.deleteTransaction(id).pipe(
      tap(() => this.clearCache())
    );
  }

  createTransaction(data: NewTransaction): Observable<Transaction> {
    return this.transactionService.createTransaction(data).pipe(
      tap(() => this.clearCache())
    );
  }

  updateTransaction(id: string, data: NewTransaction): Observable<Transaction> {
    return this.transactionService.updateTransaction(id, data).pipe(
      tap(() => this.clearCache())
    );
  }
}
