import { ChangeDetectionStrategy, Component, computed, effect, input, OnInit, output, signal } from '@angular/core';
import { PaymentType, Transaction, TransactionFilter, TransactionType } from '../../models/transaction.model';
import { Category } from '../../../categories/models/categories.model';
import { CurrencyPipe } from '@angular/common';
import { PaymentTypeMap, TransactionTypeMap } from '../../constants/transaction-map';

@Component({
  selector: 'app-transaction-table',
  imports: [CurrencyPipe],
  templateUrl: './transaction-table.html',
  styleUrl: './transaction-table.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class TransactionTable {
  transactions = input.required<Transaction[]>();
  categories = input.required<Category[]>();

  filtersChange = output<TransactionFilter>();

  searchTerm = signal<string>('');
  selectedType = signal<TransactionType | null>(null);
  minAmount = signal<number | null>(null);
  maxAmount = signal<number | null>(null);
  startDate = signal<string | null>(null);
  endDate = signal<string | null>(null);
  selectedCategories = signal<string[]>([]);
  selectedPaymentType = signal<PaymentType | null>(null);

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

  constructor() {
    effect(() => {
      this.filtersChange.emit(this.filters());
    });
  }
  
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
    const value = (e.target as HTMLSelectElement).value;
    this.selectedType.set(Number(value) || null);
  }

  onChangePaymentType(e: Event) {
    const value = (e.target as HTMLSelectElement).value;
    this.selectedPaymentType.set(Number(value) || null);
  }
}
