import { Component, computed, effect, input, output, signal } from '@angular/core';
import { NewTransaction } from '../../models/transaction.model';
import { Field, FieldState, form, required, submit, validate } from '@angular/forms/signals';
import { Category } from '../../../categories/models/categories.model';
import { TransactionTypeMap, PaymentTypeMap } from '../../constants/transaction-map';
import { EnumSelect } from '../../../../shared/components/enum-select/enum-select';

@Component({
  selector: 'app-transaction-form',
  imports: [Field, EnumSelect],
  templateUrl: './transaction-form.html',
  styleUrl: './transaction-form.css',
})
export class TransactionForm {
  isEdit = input(false);
  model = input.required<NewTransaction>();
  save = output<NewTransaction>();
  cancel = output<void>();
  categories = input.required<Category[]>();
  TransactionTypeMap = TransactionTypeMap;
  PaymentTypeMap = PaymentTypeMap;

  isLoading = signal(false);

  private readonly modelSignal = signal({} as NewTransaction);
  protected readonly transactionForm = form(this.modelSignal, schema => {
    required(schema.name, { message: "Name is required"});
    required(schema.amount, { message: 'Amount is required' });
    required(schema.category, { message: "Category is required"});
    required(schema.transactionType, { message: 'Transaction type is required' });
    required(schema.paymentType, { message: 'Payment type is required' });

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

    submit(this.transactionForm, async () => {
      this.isLoading.set(true);
      const data = this.modelSignal();
      console.log(data);

      try {
        if (this.transactionForm().valid()) {
          this.save.emit(data as NewTransaction);
          this.isLoading.set(false);
        }
      } catch (err) {
        console.error(`Failed to ${this.isEdit() ? "edit" : "add"}: ${err}`);
        this.isLoading.set(false);
      }
    })
  }

  onCancel(): void {
    this.cancel.emit();
  }

  protected fieldErrors = {
    name: this.createErrorSignal(() => this.transactionForm.name()),
    amount: this.createErrorSignal(() => this.transactionForm.amount()),
    category: this.createErrorSignal(() => this.transactionForm.category()),
    transactionType: this.createErrorSignal(() => this.transactionForm.transactionType()),
    paymentType: this.createErrorSignal(() => this.transactionForm.paymentType())
  }

  private createErrorSignal(field: () => FieldState<string | number>) {
    return computed(() => this.setShowError(field()));
  };

  private setShowError(field: FieldState<string | number>) {
    return field.invalid() && field.touched();
  };
}