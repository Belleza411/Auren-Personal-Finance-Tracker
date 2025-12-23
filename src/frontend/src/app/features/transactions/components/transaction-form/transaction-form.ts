import { Component, effect, input, output, signal } from '@angular/core';
import { NewTransaction } from '../../models/transaction.model';
import { Field, form } from '@angular/forms/signals';
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
  isLoading = signal(false);
  isEdit = input(false);
  model = input.required<NewTransaction>();
  save = output<NewTransaction>();
  cancel = output<void>();
  categories = input.required<Category[]>();
  TransactionTypeMap = TransactionTypeMap;
  PaymentTypeMap = PaymentTypeMap;

  private readonly modelSignal = signal({} as NewTransaction);
  protected readonly transactionForm = form(this.modelSignal);

  constructor() {
    effect(() => {
      this.modelSignal.set(this.model());
    });
  }

  onSave(): void {
    if (this.transactionForm().valid()) {
      this.save.emit(this.transactionForm().value() as NewTransaction);
    }
  }

  onCancel(): void {
    this.cancel.emit();
  }
}