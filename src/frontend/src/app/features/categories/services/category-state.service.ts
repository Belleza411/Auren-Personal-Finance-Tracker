import { inject, Injectable } from '@angular/core';
import { CacheStateService } from '../../../core/services/cache-state.service';
import { PagedResult } from '../../transactions/models/transaction.model';
import { Category, CategoryFilter, NewCategory } from '../models/categories.model';
import { Observable, tap } from 'rxjs';
import { createHttpParams } from '../../../shared/utils/http-params.util';
import { CategoryService } from './category.service';

@Injectable({
  providedIn: 'root',
})
export class CategoryStateService extends CacheStateService<PagedResult<Category>, CategoryFilter> {
  protected override ttl: number = 120_000;
  protected override readonly initialKey: string = "auren:categories";

  private categoryService = inject(CategoryService);

  getCategories(
    filters?: Partial<CategoryFilter>,
    pageSize?: number,
    pageNumber?: number
  ): Observable<PagedResult<Category>> {
    const params = createHttpParams(filters, pageSize, pageNumber);
    const key = this.generateCacheKey(filters, pageSize, pageNumber)
    return this.getFromCache(key, () => this.categoryService.getAllCategories(params));
  }

  deleteCategory(id: string): Observable<void> {
    return this.categoryService.deleteCategory(id).pipe(
      tap(() => this.clearCache())
    );
  }

  createCategory(data: NewCategory): Observable<Category> {
    return this.categoryService.createCategory(data).pipe(
      tap(() => this.clearCache())
    )
  }

  updateCategory(id: string, data: NewCategory): Observable<Category> {
    return this.categoryService.updateCategory(id, data).pipe(
      tap(() => this.clearCache())
    )
  }
}
