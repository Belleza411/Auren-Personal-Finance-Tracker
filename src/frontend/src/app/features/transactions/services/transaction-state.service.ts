import { inject, Injectable } from '@angular/core';
import { TransactionService } from './transaction.service';
import { Observable, of, shareReplay } from 'rxjs';
import { PagedResult, Transaction, TransactionFilter } from '../models/transaction.model';
import { createHttpParams } from '../../../shared/utils/http-params.util';

@Injectable({
  providedIn: 'root',
})
export class TransactionStateService {
  transactionService = inject(TransactionService);
  private readonly ttl = 60_000;
  private cache = new Map<string, { timestamp: number, data$: Observable<PagedResult<Transaction>>}>();

  getTransactions(filters?: Partial<TransactionFilter>, pageSize?: number, pageNumber?: number): Observable<PagedResult<Transaction>> {    
    const params = createHttpParams(filters, pageSize, pageNumber);

    // Create a unique cache key based on the filters and pagination parameters
    const cacheKey = JSON.stringify({ filters, pageSize, pageNumber })

    // Get the cached key
    const cached = this.cache.get(cacheKey)

    // If cached data exists and is still valid, return cached data
    if(cached && Date.now() - cached.timestamp < this.ttl) {
      return cached.data$;
    }

    // If the cached data is expired, remove it from the cache, and fetch new data
    const requests$ = this.transactionService
      .getAllTransactions(params) 
        .pipe(
          shareReplay(1)
        )
        
    // Store the new data in the cache with the current timestamp
    this.cache.set(cacheKey, { timestamp: Date.now(), data$: requests$ });
    
        
    return requests$;
  }

  clearCache() {
    this.cache.clear();     
  }

  clearCacheByFilter(
    filters?: Partial<TransactionFilter>
  ) {
    const prefix = JSON.stringify({ filters });

    for (const key of this.cache.keys()) {
      if (key.includes(prefix)) {
        this.cache.delete(key);
      }
    }
  }
}
