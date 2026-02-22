import { ChangeDetectionStrategy, Component, computed, DestroyRef, effect, inject, input, output, signal } from '@angular/core';
import { CurrencyPipe, UpperCasePipe } from '@angular/common';
import { outputFromObservable, takeUntilDestroyed, toObservable } from '@angular/core/rxjs-interop';
import { debounceTime, distinctUntilChanged } from 'rxjs';

import { PaymentType, PaymentTypeFilterOption, Transaction, TransactionFilter, TransactionType, TransactionTypeFilterOption } from '../../models/transaction.model';
import { Category } from '../../../categories/models/categories.model';
import { PaginationComponent } from "../../../../shared/ui/pagination/pagination";
import { COMPACT_TRANSACTION_COLUMNS, FULL_TRANSACTION_COLUMNS } from '../../models/transaction-column.model';
import { TransactionAmountSignPipe } from '../../pipes/transaction-amount-sign.pipe';
import { TransactionTypeColorPipe } from '../../pipes/transaction-type-color.pipe';
import { DATE_FILTER_LABEL_OPTION, paymentTypeOptions, transactionTypeOptions } from '../../../../shared/constants/type-options';
import { AddWhitespacePipe } from '../../pipes/add-whitespace.pipe';
import { Dropdown } from "../../../../shared/ui/filters/components/dropdown/dropdown";
import { DropdownWithModal } from "../../../../shared/ui/filters/components/dropdown-with-modal/dropdown-with-modal";

@Component({
  selector: 'app-transaction-table',
  imports: [
    CurrencyPipe,
    PaginationComponent,
    UpperCasePipe,
    TransactionAmountSignPipe,
    TransactionTypeColorPipe,
    AddWhitespacePipe,
    Dropdown,
    DropdownWithModal
],
  templateUrl: './transaction-table.html',
  styleUrl: './transaction-table.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: {
    '[class.compact]': "variant() === 'compact'"
  }
})
export class TransactionTable {
  private destroyRef = inject(DestroyRef);

  transactions = input.required<Transaction[]>();
  categories = input<Category[]>();
  isLoading = input<boolean>();
  variant = input<'full' | 'compact'>('full');

  columns = computed(() =>
    this.variant() === 'compact'
      ? COMPACT_TRANSACTION_COLUMNS
      : FULL_TRANSACTION_COLUMNS
  );
  
  showFilters = computed(() => this.variant() === 'full');
  showPagination = computed(() => this.variant() === 'full');

  pageNumberChange = output<number>();
  pageSizeChange = output<number>();

  pageNumber = signal(1);
  pageSize = signal(10);
  totalCount = input<number>();

  selectedTransactionType: TransactionTypeFilterOption = "All Types";
  selectedPaymentType: PaymentTypeFilterOption = "All Payment Method";
  transactionTypeOptions = transactionTypeOptions;
  paymentTypeOptions = paymentTypeOptions;

  dateFilterLabelOption = DATE_FILTER_LABEL_OPTION;
 
  pageSizeOptions: number[] = [10, 20, 30, 40, 50];

  isCategoryOpen = signal(false);

  toogleCategoryModal() {
    this.isCategoryOpen.update(open => !open);
  }
  
  openModalId = signal<string | null>(null);

  toggleModalId(id: string) {
    this.openModalId.update(current =>
      current === id ? null : id
    );
  }
  
  delete = output<string>();
  edit = output<string>(); 

  filters = signal<TransactionFilter>({
    searchTerm: '',
    transactionType: null,
    startDate: null,
    endDate: null,
    category: [],
    paymentType: null
  });

  filtersChange = outputFromObservable(
    toObservable(this.filters).pipe(
      debounceTime(300),
      distinctUntilChanged((a, b) => JSON.stringify(a) === JSON.stringify(b)),
      takeUntilDestroyed(this.destroyRef)
    )
  );

  setFilter<K extends keyof TransactionFilter>(key: K, value: TransactionFilter[K]) {
    this.filters.update(f => ({ ...f, [key]: value }));
  }

  onDelete(id: string) {
    this.delete.emit(id);
  }

  onEdit(id: string) {
    this.edit.emit(id);
  }

  onSearchChange(e: Event) {
    const value = (e.target as HTMLInputElement).value;
    this.setFilter('searchTerm', value);
  }
  
  onCategoryToggle(category: string, checked: boolean) {
    this.setFilter('category', checked
      ? [...this.filters().category, category]
      : this.filters().category.filter(c => c !== category)
    );
  }

  onChangeTransactionType(value: TransactionTypeFilterOption) {
    this.setFilter('transactionType', value === "All Types" 
      ? null 
      : value as TransactionType);
  }

  onChangePaymentType(value: PaymentTypeFilterOption) {
    this.setFilter('paymentType', value === "All Payment Method" 
      ? null 
      : value as PaymentType);
  }

  onStartDateChange(value: string | Date | null) {
    this.setFilter('startDate', value ?? null);
  }

  onEndDateChange(value: string | Date | null) {
    this.setFilter('endDate', value ?? null);
  }

  clearFilter() {
    this.filters.set({
      searchTerm: '',
      transactionType: null,
      startDate: null,
      endDate: null,
      category: [],
      paymentType: null
    })
  }

  clearDateFilter() {
    this.filters.update(f => ({ ...f, startDate: null, endDate: null }));
  }

  clearAmountFilter() {
    this.filters.update(f => ({ ...f, minAmount: null, maxAmount: null }));
  }

  hasActiveFilters = computed(() => {
    const hasSearch = this.filters().searchTerm.trim().length !== 0;
    const hasType = this.filters().transactionType !== null;
    const hasCategory = this.filters().category.length !== 0;
    const hasPayment = this.filters().paymentType !== null;
    const hasDate = this.filters().startDate !== null || this.filters().endDate !== null;
    return hasSearch || hasType || hasCategory || hasPayment || hasDate;
  });

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
    return this.transactions().length > 10;
  }
}  
