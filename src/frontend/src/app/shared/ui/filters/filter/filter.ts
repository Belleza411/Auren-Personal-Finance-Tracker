import { ChangeDetectionStrategy, Component, computed, effect, input, output, signal, untracked } from '@angular/core';
import { debounceTime } from 'rxjs/internal/operators/debounceTime';
import { toObservable, toSignal } from '@angular/core/rxjs-interop';
import { distinctUntilChanged } from 'rxjs';

import { Dropdown } from "../components/dropdown/dropdown";
import { DATE_FILTER_LABEL_OPTION, paymentTypeOptions, transactionTypeOptions } from '../../../constants/type-options';
import { Category } from '../../../../features/categories/models/categories.model';
import { DropdownWithModal } from "../components/dropdown-with-modal/dropdown-with-modal";
import { FilterKindConfig } from '../models/filter.model';
import { 
  TransactionTypeFilterOption,
  PaymentTypeFilterOption,
  TransactionType,
  PaymentType 
} from '../../../../features/transactions/models/transaction.model';

@Component({
  selector: 'app-filter',
  imports: [Dropdown, DropdownWithModal],
  templateUrl: './filter.html',
  styleUrl: './filter.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class Filter<T extends object> {
  filters = input.required<T>();
  config = input.required<FilterKindConfig<T>[]>();
  categories = input.required<Category[]>();
  
  filtersChange = output<T>();
  readonly currentFilters = computed(() => this.filters());
  readonly searchInput = signal<string>('');

  readonly debouncedSearch = toSignal(
    toObservable(this.searchInput).pipe(
      debounceTime(400),
      distinctUntilChanged()
    ),
    {
      initialValue: '',
    }
  )

  constructor() {
    effect(() => {
      const searchTerm = this.debouncedSearch();
      const current = untracked(() =>
        this.getFilter('searchTerm' as keyof T)
      );

      if (searchTerm === current) return;

      this.setFilter('searchTerm' as keyof T, searchTerm as T[keyof T]);
    });
  }

  isFilterOpen = signal(false);
  isCategoryOpen = signal(false);

  toggleCategoryModal() {
    this.isCategoryOpen.update(open => !open);
  }

  toggleFilter() {
    this.isFilterOpen.update(open => !open);
  }

  transactionTypeOptions = transactionTypeOptions;
  paymentTypeOptions = paymentTypeOptions;
  readonly selectedTransactionType = computed<TransactionTypeFilterOption>(() => 
    this.transactionType() ?? transactionTypeOptions[0].value
  );
  readonly selectedPaymentType = computed<PaymentTypeFilterOption>(() => 
    this.paymentType() ?? paymentTypeOptions[0].value
  );
  
  dateFilterLabelOption = DATE_FILTER_LABEL_OPTION;

  readonly hasActiveFilters = computed(() =>
    Object.values(this.currentFilters() ?? {}).some(v => {
      if (Array.isArray(v)) return v.length > 0;
      if (v === null || v === undefined || v === '') return false;
      return true;
    })
  );

  readonly transactionType = computed(() => this.getFilter('transactionType' as keyof T) as TransactionType | null);
  readonly paymentType = computed(() => this.getFilter('paymentType' as keyof T) as PaymentType | null);
  readonly categoriesSelected = computed(() => this.getFilter('category' as keyof T) as string[] ?? []);
  readonly startDate = computed(() => this.getFilter('startDate' as keyof T) as Date | null);
  readonly endDate = computed(() => this.getFilter('endDate' as keyof T) as Date | null);
  readonly searchTerm = computed(() => this.getFilter('searchTerm' as keyof T) as string);

  setFilter<K extends keyof T>(key: K, value: T[K]) {
    this.filtersChange.emit({ ...this.currentFilters(), [key]: value });
  }

  getFilter<K extends keyof T>(key: K): T[K] {
    return this.currentFilters()[key];
  }

  onChangeTransactionType(value: TransactionTypeFilterOption) {
    this.setFilter('transactionType' as keyof T, (value === "All Types" 
      ? null 
      : value as TransactionType) as T[keyof T]);
  }

  onChangePaymentType(value: PaymentTypeFilterOption) {
      this.setFilter('paymentType' as keyof T, (value === "All Payment Method" 
        ? null 
        : value as PaymentType) as T[keyof T]);
  }

  onCategoryToggle(category: string, checked: boolean) {
    const current = (this.filters() as Record<string, unknown>)['category'] as string[] ?? [];
    this.setFilter('category' as keyof T, (checked
      ? [...current, category]
      : current.filter(c => c !== category)) as T[keyof T]
    );
  }

  onStartDateChange(value: string | Date | null) {
    this.setFilter('startDate' as keyof T, (value ?? null) as T[keyof T]);
  }

  onEndDateChange(value: string | Date | null) {
    this.setFilter('endDate' as keyof T, (value ?? null) as T[keyof T]);
  }

  clearFilter() {
    const cleared = Object.fromEntries(
      Object.entries(this.currentFilters() ?? {}).map(([k, v]) => {
        if (Array.isArray(v)) return [k, []];
        if (typeof v === 'string') return [k, ''];
        return [k, null];
      })
    ) as T;

    this.searchInput.set('');
    this.filtersChange.emit(cleared);
  }

  clearDateFilter() {
    this.filtersChange.emit({ ...this.currentFilters(), startDate: null, endDate: null });
  }
}
