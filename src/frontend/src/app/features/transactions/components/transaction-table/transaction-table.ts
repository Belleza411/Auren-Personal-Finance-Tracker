import { ChangeDetectionStrategy, Component, computed, DestroyRef, ElementRef, inject, input, output, signal, viewChild } from '@angular/core';
import { PaymentType, Transaction, TransactionFilter, TransactionType } from '../../models/transaction.model';
import { Category } from '../../../categories/models/categories.model';
import { CurrencyPipe } from '@angular/common';
import { PaymentTypeMap, TransactionTypeMap } from '../../constants/transaction-map';
import { outputFromObservable, takeUntilDestroyed, toObservable } from '@angular/core/rxjs-interop';
import { debounceTime, distinctUntilChanged } from 'rxjs';

@Component({
  selector: 'app-transaction-table',
  imports: [CurrencyPipe],
  templateUrl: './transaction-table.html',
  styleUrl: './transaction-table.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class TransactionTable {
  private destroyRef = inject(DestroyRef);

  transactions = input.required<Transaction[]>();
  categories = input.required<Category[]>();

  searchTerm = signal<string>('');
  selectedType = signal<TransactionType | null>(null);
  minAmount = signal<number | null>(null);
  maxAmount = signal<number | null>(null);
  startDate = signal<string | null>(null);
  endDate = signal<string | null>(null);
  selectedCategories = signal<string[]>([]);
  selectedPaymentType = signal<PaymentType | null>(null);

  transactionTypeOptions: string[] = ['All Types', 'Income', 'Expense'];
  paymentTypeOptions: string[] = ['All Payment Method', 'Cash', 'Credit Card', 'Bank Transfer', 'Other'];

  modals = signal({
    amount: false,
    date: false,
    category: false
  })

  toggleModal(modalName: 'amount' | 'date' | 'category') {
    this.modals.update(modals => ({
      ...modals,
      [modalName]: !modals[modalName]
    }));
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

  protected TransactionTypeMap = TransactionTypeMap;
  protected PaymentTypeMap = PaymentTypeMap;
  protected categoryMap = computed(() => {
    return new Map(
      this.categories().map(c => [c.categoryId, c.name])
    );
  });

  onCategoryToggle(category: string, checked: boolean) {
    this.selectedCategories.update(categories =>
      checked
        ? [...categories, category]
        : categories.filter(c => c !== category)
    );  
  }

  onChangeType(e: Event) {
    const value = Number((e.target as HTMLSelectElement).value);
    this.selectedType.set(value === 0 ? null : value);
  }

  onChangePaymentType(e: Event) {
    const value = Number((e.target as HTMLSelectElement).value);
    this.selectedPaymentType.set(value === 0 ? null : value);
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
}
