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
  
  delete = output<string>();
  edit = output<string>(); 
  
  isAmountModalVisible = signal<boolean>(false);
  isDateModalVisible = signal<boolean>(false);
  isCategoryModalVisible = signal<boolean>(false);

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

  toggleAmountModal() {
    this.isAmountModalVisible.update(v => !v);
  }

  toggleDateModal() {
    this.isDateModalVisible.update(v => !v);
  }

  toggleCategoryModal() {
    this.isCategoryModalVisible.update(v => !v);
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
