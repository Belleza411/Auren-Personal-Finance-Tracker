import { Component, computed, DestroyRef, inject, input, output, signal } from '@angular/core';
import { Category, CategoryFilter } from '../../models/categories.model';
import { TransactionType } from '../../../transactions/models/transaction.model';
import { outputFromObservable, toObservable, takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { debounceTime, distinctUntilChanged } from 'rxjs';
import { PaginationComponent } from "../../../../shared/components/pagination/pagination";
import { UpperCasePipe } from '@angular/common';

@Component({
  selector: 'app-category-table',
  imports: [PaginationComponent, UpperCasePipe],
  templateUrl: './category-table.html',
  styleUrl: './category-table.css',
})
export class CategoryTable {
  private destroyRef = inject(DestroyRef);

  categories = input.required<Category[]>();
  isLoading = input.required<boolean>();

  searchTerm = signal<string>('');
  selectedType = signal<TransactionType | null>(null);

  pageNumberChange = output<number>();
  pageSizeChange = output<number>();

  pageNumber = signal(1);
  pageSize = signal(10);
  totalCount = input.required<number>();

  pageSizeOptions: number[] = [10, 20, 30, 40, 50];
  transactionTypeOptions: string[] = ['All Types', 'Income', 'Expense'];

  delete = output<string>();
  edit = output<string>();

  openModalId = signal<string | null>(null);

  toggleModalId(id: string) {
    this.openModalId.update(current =>
      current === id ? null : id
    );
  }

  private readonly filters = computed<CategoryFilter>(() => ({
    searchTerm: this.searchTerm(),
    transactionType: this.selectedType()
  }));

  filtersChange = outputFromObservable(
    toObservable(this.filters).pipe(
      debounceTime(300),
      distinctUntilChanged((a, b) => JSON.stringify(a) === JSON.stringify(b)),
      takeUntilDestroyed(this.destroyRef)
    )
  );

  onDelete(id: string) {
    this.delete.emit(id);
  }

  onEdit(id: string) {
    this.edit.emit(id);
  }

  onChangeType(e: Event) {
    const value = (e.target as HTMLSelectElement).value;
    this.selectedType.set(value === 'All Types' ? null : value as TransactionType);
  }

  clearFilter() {
    this.searchTerm.set('');
    this.selectedType.set(null);
  }

  hasActiveFilters = computed(() => {
    const hasSearch = this.searchTerm().trim().length !== 0;
    const hasType = this.selectedType() !== null;

    return hasSearch || hasType;
  })

  onPageChange(page: number): void {
    this.pageNumber.set(page);
    this.pageNumberChange.emit(page);
  }

  onPageSizeChange(size: number): void {
    this.pageSize.set(size);
    this.pageNumber.set(1);
    this.pageSizeChange.emit(size);
    this.pageNumberChange.emit(1); 
  }

  isShowPagination(): boolean {
    return this.categories().length > 5;
  }

  isIncome(c: Category): boolean {
    return c.transactionType === "Income";
  }
}
