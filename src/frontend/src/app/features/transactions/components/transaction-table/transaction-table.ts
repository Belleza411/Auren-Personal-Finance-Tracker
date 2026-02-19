import { ChangeDetectionStrategy, Component, computed, DestroyRef, inject, input, output, signal } from '@angular/core';
import { CurrencyPipe, UpperCasePipe } from '@angular/common';
import { outputFromObservable, takeUntilDestroyed, toObservable } from '@angular/core/rxjs-interop';
import { debounceTime, distinctUntilChanged } from 'rxjs';

import { PaymentType, Transaction, TransactionFilter, TransactionType } from '../../models/transaction.model';
import { Category } from '../../../categories/models/categories.model';
import { PaginationComponent } from "../../../../shared/components/pagination/pagination";
import { COMPACT_TRANSACTION_COLUMNS, FULL_TRANSACTION_COLUMNS } from '../../models/transaction-column.model';

@Component({
  selector: 'app-transaction-table',
  imports: [CurrencyPipe, PaginationComponent, UpperCasePipe],
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

  searchTerm = signal<string>('');
  selectedType = signal<TransactionType | null>(null);
  minAmount = signal<number | null>(null);
  maxAmount = signal<number | null>(null);
  startDate = signal<string | null>(null);
  endDate = signal<string | null>(null);
  selectedCategories = signal<string[]>([]);
  selectedPaymentType = signal<PaymentType | null>(null);

  pageNumberChange = output<number>();
  pageSizeChange = output<number>();

  pageNumber = signal(1);
  pageSize = signal(10);
  totalCount = input<number>();

  transactionTypeOptions: string[] = ['All Types', 'Income', 'Expense'];
  paymentTypeOptions: string[] = ['All Payment Method', 'Cash', 'Credit Card', 'Bank Transfer', 'Other'];
  pageSizeOptions: number[] = [10, 20, 30, 40, 50];

  modals = signal({
    amount: false,
    date: false,
    category: false
  });
  
  toggleModal(modalName: 'amount' | 'date' | 'category') {
    this.modals.update(modals => ({
      ...modals,
      [modalName]: !modals[modalName]
    }));
  }

  openModalId = signal<string | null>(null);

  toggleModalId(id: string) {
    this.openModalId.update(current =>
      current === id ? null : id
    );
  }
  
  delete = output<string>();
  edit = output<string>(); 

  private readonly filters = computed<TransactionFilter>(() => ({
    searchTerm: this.searchTerm(),
    transactionType: this.selectedType(),
    minAmount: this.minAmount(),
    maxAmount: this.maxAmount(),
    startDate: this.startDate(),
    endDate: this.endDate(),
    category: this.selectedCategories(),
    paymentType: this.selectedPaymentType()
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
  
  onCategoryToggle(category: string, checked: boolean) {
    this.selectedCategories.update(categories =>
      checked
        ? [...categories, category]
        : categories.filter(c => c !== category)
    );  
  }

  onChangeType(e: Event) {
    const value = (e.target as HTMLSelectElement).value;
    this.selectedType.set(value === "All Types" ? null : value as TransactionType);
  }

  onChangePaymentType(e: Event) {
    const value = (e.target as HTMLSelectElement).value;
    this.selectedPaymentType.set(value === "All Payment Method" ? null : value as PaymentType);
  }

  clearFilter() {
    this.searchTerm.set('');
    this.selectedType.set(null);
    this.minAmount.set(null);
    this.maxAmount.set(null);
    this.startDate.set(null);
    this.endDate.set(null);
    this.selectedCategories.set([]);
    this.selectedPaymentType.set(null);
  }

  clearDateFilter() {
    this.startDate.set(null);
    this.endDate.set(null);
  }

  clearAmountFilter() {
    this.minAmount.set(null);
    this.maxAmount.set(null);
  }

  hasActiveFilters = computed(() => {
    const hasSearch = this.searchTerm().trim().length !== 0;
    const hasType = this.selectedType() !== null;
    const hasCategory = this.selectedCategories().length !== 0;
    const hasPayment = this.selectedPaymentType() !== null;
    const hasAmount = this.minAmount() !== null|| this.maxAmount() !== null;
    const hasDate = this.startDate() !== null || this.endDate() !== null;
    return hasSearch || hasType || hasCategory || hasPayment || hasAmount || hasDate;
  });

  formatDate(event: Event) {
    const value = (event.target as HTMLInputElement).value;
    const date = new Date(value);
    return date.toLocaleDateString('en-US', {
      month: 'short', 
      day: 'numeric', 
      year: 'numeric', 
    })
  }

  isIncome(t: Transaction): boolean {
    return t.transactionType === "Income";
  }

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
    return this.transactions().length > 5;
  }
}
