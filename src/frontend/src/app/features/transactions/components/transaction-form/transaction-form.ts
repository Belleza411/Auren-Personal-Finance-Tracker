import { ChangeDetectionStrategy, Component, computed, effect, input, output, signal } from '@angular/core';
import { NewTransaction } from '../../models/transaction.model';
import { FieldState, form, FormField, required, submit, validate } from '@angular/forms/signals';
import { Category } from '../../../categories/models/categories.model';

@Component({
  selector: 'app-transaction-form',
  imports: [FormField],
  templateUrl: './transaction-form.html',
  styleUrl: './transaction-form.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class TransactionForm {
  isEdit = input(false);
  model = input.required<NewTransaction>();
  save = output<NewTransaction>();
  cancel = output<void>();
  categories = input.required<Category[]>();

  isLoading = signal(false);

  paymentTypes = [
    { value: 'Cash', label: 'Cash' },
    { value: 'CreditCard', label: 'Credit Card' },
    { value: 'BankTransfer', label: 'Bank Transfer' },
    { value: 'Other', label: 'Other' }
  ];

  private readonly modelSignal = signal({} as NewTransaction);
  protected readonly transactionForm = form(this.modelSignal, schema => {
    required(schema.name, { message: "Name is required"});
    required(schema.amount, { message: 'Amount is required' });
    required(schema.category, { message: "Category is required"});
    required(schema.transactionType, { message: 'Transaction type is required' });
    required(schema.paymentType, { message: 'Payment type is required' })
    required(schema.transactionDate, { message: 'Transaction date is required'});

    validate(schema.amount, ({ value }) => {
      const amount = value();
      if (amount <= 0) {
        return {
          kind: 'invalidAmount',
          message: 'Amount must be greater than 0'
        };
      }
      return null;
    });
  });

  constructor() {
    effect(() => {
      this.modelSignal.set(this.model());
    });
  }

  onSubmit(event: Event) {
    event.preventDefault();

    const categoryName = this.categories().find(c => c.categoryId === this.modelSignal().category)?.name ?? '';

    const updatedModel: NewTransaction = {
      ...this.modelSignal(),
      category: categoryName,
    } 
    
    submit(this.transactionForm, async () => {
      this.isLoading.set(true);
      this.save.emit(updatedModel);
    })
  }

  onCancel(): void {
    this.cancel.emit();
  }

  protected readonly fieldErrors = {
    name: this.createErrorSignal(() => this.transactionForm.name()),
    amount: this.createErrorSignal(() => this.transactionForm.amount()),
    category: this.createErrorSignal(() => this.transactionForm.category()),
    transactionType: this.createErrorSignal(() => this.transactionForm.transactionType()),
    paymentType: this.createErrorSignal(() => this.transactionForm.paymentType()),
    transactionDate: this.createErrorSignal(() => this.transactionForm.transactionDate())
  }

  private createErrorSignal<T>(field: () => FieldState<T>) {
    return computed(() => this.setShowError(field()));
  };

  private setShowError<T>(field: FieldState<T>) {
    return field.invalid() && field.touched();
  };
}