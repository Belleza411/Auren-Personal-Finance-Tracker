import { inject, Injectable, OnDestroy } from '@angular/core';
import { TransactionService } from './transaction.service';
import { Observable, of, shareReplay, tap } from 'rxjs';
import { NewTransaction, PagedResult, Transaction, TransactionFilter } from '../models/transaction.model';
import { createHttpParams } from '../../../shared/utils/http-params.util';
import { CacheEntry } from '../../../shared/models/cache.model';

@Injectable({
  providedIn: 'root',
})
export class TransactionStateService implements OnDestroy {
  transactionService = inject(TransactionService);
  private readonly ttl = 120_000;
  private readonly initialKey = "auren:transactions";
  private readonly cache = new Map<string, CacheEntry<PagedResult<Transaction>>>();
  private evictionInterval = setInterval(() => this.evictExpired(), this.ttl);

  ngOnDestroy(): void {
    clearInterval(this.evictionInterval);
  }

  getTransactions(
    filters?: Partial<TransactionFilter>,
    pageSize?: number,
    pageNumber?: number
  ): Observable<PagedResult<Transaction>> {    
    const params = createHttpParams(filters, pageSize, pageNumber);

    // Create a unique cache key based on the filters and pagination parameters
    const cacheKey = this.generateCacheKey({ filters, pageSize, pageNumber });

    // Get the cached key
    const cached = this.cache.get(cacheKey)

    // If cached data exists and is still valid, return cached data
    if(cached && !this.isExpired(cached)) {
      return cached.data$;
    }

    // If cached data exists but is expired, remove it from the cache
    this.cache.delete(cacheKey);

    // If the cached data is expired, remove it from the cache, and fetch new data
    const data$ = this.transactionService
      .getAllTransactions(params) 
        .pipe(
          tap({
            error: () => this.cache.delete(cacheKey) // Remove from cache if the request fails 
          }),
          shareReplay(1)
        )
        
    // Store the new data in the cache with the current timestamp
    this.cache.set(cacheKey, { timestamp: Date.now(), data$: data$ });
        
    return data$;
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

  clearCache() {
    this.cache.clear();     
  }

  clearCacheByFilter(filters?: Partial<TransactionFilter>): void {
    // Use a stable key prefix so we match accurately without substring false positives
    const filterSegment = this.stableStringify(filters ?? {});
    const prefix = `${this.initialKey}:${filterSegment}:`;

    for (const key of this.cache.keys()) {
      if (key.startsWith(prefix)) {
        this.cache.delete(key);
      }
    }
  }

  private generateCacheKey(params: { filters?: Partial<TransactionFilter>, pageSize?: number, pageNumber?: number }) {
    const { filters = {}, pageSize = 10, pageNumber = 1 } = params;

    const filtersKey = this.stableStringify(filters);

    return `${this.initialKey}:${filtersKey}:${pageSize}:${pageNumber}`;
  }

  private isExpired(entry: CacheEntry<PagedResult<Transaction>>): boolean {
    return Date.now() - entry.timestamp >= this.ttl;
  }


  // Sort keys before serializing to ensure consistent cache keys for the same filter objects regardless of key order
  private stableStringify(obj: object): string {
    return JSON.stringify(obj, Object.keys(obj).sort());
  }

  private evictExpired(): void {
    for (const [key, entry] of this.cache.entries()) {
      if (this.isExpired(entry)) {
        this.cache.delete(key);
      }
    }
  }
}
