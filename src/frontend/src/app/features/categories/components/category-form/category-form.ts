import { Component, computed, effect, input, output, signal } from '@angular/core';
import { NewCategory } from '../../models/categories.model';
import { FieldState, form, required, submit, FormField } from '@angular/forms/signals';
import { TransactionType } from '../../../transactions/models/transaction.model';

@Component({
  selector: 'app-category-form',
  imports: [FormField],
  templateUrl: './category-form.html',
  styleUrl: './category-form.css',
})
export class CategoryForm {
  isEdit = input(false);
  model = input.required<NewCategory>();
  save = output<NewCategory>();
  cancel = output<void>();
  isLoading = signal(false);

  TransactionType = TransactionType;

  constructor() {
    effect(() => {
      this.modelSignal.set(this.model());
    })
  }

  private readonly modelSignal = signal({} as NewCategory);
  protected readonly categoryForm = form(this.modelSignal, schema => {
    required(schema.name, { message: "Name is required "});
    required(schema.transactionType, { message: 'Transaction type is required' });
  })

  onSubmit(event: Event) {
    event.preventDefault();

    submit(this.categoryForm, async () => {
      this.isLoading.set(true);
      this.save.emit(this.modelSignal());
    })
  }

  onCancel(): void {
    this.cancel.emit();
  }

  protected readonly fieldErrors = {
    name: this.createErrorSignal(() => this.categoryForm.name()),
    transactionType: this.createErrorSignal(() => this.categoryForm.transactionType())
  }

  private createErrorSignal<T>(field: () => FieldState<T>) {
    return computed(() => this.setShowError(field()));
  };

  private setShowError<T>(field: FieldState<T>) {
    return field.invalid() && field.touched();
  };
}
