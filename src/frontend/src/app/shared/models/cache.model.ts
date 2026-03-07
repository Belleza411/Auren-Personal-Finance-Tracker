import { Observable } from "rxjs";

export interface CacheEntry<T> {
    timestamp: number;
    data$: Observable<T>;
}