import { Injectable, OnDestroy, OnInit } from '@angular/core';
import { CacheEntry } from '../../shared/models/cache.model';
import { Observable, shareReplay, tap } from 'rxjs';

@Injectable()
export abstract class CacheStateService<T, TFilter extends object = object> implements OnInit, OnDestroy {
  protected abstract ttl: number;
  protected abstract initialKey: string;

  private readonly cache = new Map<string, CacheEntry<T>>();
  private evictionInterval?: number;

  ngOnInit(): void {
    this.evictionInterval = setInterval(() => this.evictExpired(), this.ttl);
  }

  ngOnDestroy(): void {
    if (this.evictionInterval !== undefined) {
      clearInterval(this.evictionInterval);
    }
  }

  protected getFromCache(
    key: string,
    fetcher: () => Observable<T>
  ): Observable<T> {
    const cached = this.cache.get(key);

    if (cached && !this.isExpired(cached)) {
      return cached.data$;
    }

    this.cache.delete(key);

    const data$ = fetcher().pipe(
      tap({ error: () => this.cache.delete(key) }),
      shareReplay(1)
    );

    this.cache.set(key, { timestamp: Date.now(), data$ });

    return data$;
  }

  protected invalidateOnSuccess<R>(source$: Observable<R>): Observable<R> {
    return source$.pipe(tap(() => this.clearCache()));
  }

  clearCache(): void {
    this.cache.clear();
  }

  clearCacheByFilter(filters?: Partial<TFilter>): void {
    const prefix = `${this.initialKey}:${this.stableStringify(filters ?? {})}:`;
    for (const key of this.cache.keys()) {
      if (key.startsWith(prefix)) this.cache.delete(key);
    }
  }

  protected generateCacheKey(
    filters: Partial<TFilter> = {},
    pageSize = 10,
    pageNumber = 1
  ): string {
    return `${this.initialKey}:${this.stableStringify(filters)}:${pageSize}:${pageNumber}`;
  }

  protected stableStringify(obj: object): string {
    return JSON.stringify(obj, Object.keys(obj).sort());
  }

  private isExpired(entry: CacheEntry<T>): boolean {
    return Date.now() - entry.timestamp >= this.ttl;
  }

  private evictExpired(): void {
    for (const [key, entry] of this.cache.entries()) {
      if (this.isExpired(entry)) this.cache.delete(key);
    }
  }
}