import { ChangeDetectionStrategy, Component, effect, input, output, signal } from '@angular/core';
import { form, FormField, required, submit, validate } from '@angular/forms/signals';

import { NewTransaction } from '../../models/transaction.model';
import { Category } from '../../../categories/models/categories.model';
import { createFieldErrors } from '../../../../shared/utils/form-errors.util';

@Component({
  selector: 'app-transaction-form',
  imports: [FormField],
  templateUrl: './transaction-form.html',
  styleUrls: ['./transaction-form.css', '../../../../shared/styles/form.css'],
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

    validate(schema.category, ({ value, valueOf }) => {
      const categoryId = value();
      const transactionType = valueOf(schema.transactionType);

      const category = this.categories().find(c => c.id === categoryId);

      if(!category) return null;
        
      if (category.transactionType !== transactionType) {
        return {
          kind: 'invalidTransactionType',
          message: 'Invalid category type for the selected transaction type'
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

    submit(this.transactionForm, async () => {
      this.isLoading.set(true);
      const categoryName = this.categories().find(c => c.id === this.modelSignal().category)?.name ?? '';

      const updatedModel: NewTransaction = {
        ...this.modelSignal(),
        category: categoryName,
      } 

      try {
        this.save.emit(updatedModel);
      } finally {
        this.isLoading.set(false)
      }
    })
  }

  onCancel(): void {
    this.cancel.emit();
  }

  protected readonly fieldErrors = createFieldErrors({
    name:            () => this.transactionForm.name(),
    amount:          () => this.transactionForm.amount(),
    category:        () => this.transactionForm.category(),
    transactionType: () => this.transactionForm.transactionType(),
    paymentType:     () => this.transactionForm.paymentType(),
    transactionDate: () => this.transactionForm.transactionDate()
  });
}