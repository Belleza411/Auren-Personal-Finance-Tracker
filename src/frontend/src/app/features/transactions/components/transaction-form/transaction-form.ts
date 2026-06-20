import { ChangeDetectionStrategy, Component, effect, input, output, signal } from '@angular/core';
import { form, FormField, required, submit, validate } from '@angular/forms/signals';

import { NewTransaction, PaymentType } from '../../models/transaction.model';
import { Category } from '../../../categories/models/categories.model';
import { createFieldErrors } from '../../../../shared/utils/form-errors.util';
import { HlmInput } from './../../../../libs/ui/input/src';
import { HlmButton } from './../../../../libs/ui/button/src';
import { 
  HlmField,
	HlmFieldError,
	HlmFieldGroup,
	HlmFieldLabel,
} from './../../../../libs/ui/field/src';
import { HlmSelectImports } from '@spartan-ng/helm/select';
import { 
  HlmDatePicker,
  HlmDatePickerTrigger
} from './../../../../libs/ui/date-picker/src';

@Component({
  selector: 'app-transaction-form',
  imports: [
    FormField,
    HlmInput,
    HlmButton,
    HlmField,
    HlmFieldError,
    HlmFieldGroup,
    HlmFieldLabel,
    HlmSelectImports,
    HlmDatePicker,
    HlmDatePickerTrigger
  ],
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

  selectedDate = signal<Date | undefined>(undefined);

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
        const date = this.model().transactionDate;
        if (date) this.selectedDate.set(new Date(date));
    });

    effect(() => {
      const date = this.selectedDate();
      if (!date) return;
      const formatted = date.toISOString().split('T')[0];
      if (this.modelSignal().transactionDate === formatted) return;
      this.modelSignal.update(m => ({ ...m, transactionDate: formatted }));
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

  onCategoryChange(id: string | null | undefined) {
    if (!id) return;
    this.modelSignal.update(m => ({ ...m, category: id }));
  }

  onPaymentTypeChange(value: string | null | undefined) {
    if (!value) return;
    this.modelSignal.update(m => ({ ...m, paymentType: value as PaymentType }));
}

  categoryToString = (id: string): string => 
    this.categories().find(c => c.id === id)?.name ?? '';

  paymentTypeToString = (value: string): string =>
    this.paymentTypes.find(t => t.value === value)?.label ?? '';
}