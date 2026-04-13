import { Component, effect, input, output, signal } from '@angular/core';
import { NewCategory } from '../../models/categories.model';
import { form, required, submit, FormField } from '@angular/forms/signals';
import { createFieldErrors } from '../../../../shared/utils/form-errors.util';

@Component({
  selector: 'app-category-form',
  imports: [FormField],
  templateUrl: './category-form.html',
  styleUrls: ['./category-form.css', '../../../../shared/styles/form.css'],
})
export class CategoryForm {
  isEdit = input(false);
  model = input.required<NewCategory>();
  save = output<NewCategory>();
  cancel = output<void>();
  isLoading = signal(false);

  constructor() {
    effect(() => {
      const model = this.model();
      this.modelSignal.set({ ...model });
    });
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
      try {
        this.save.emit({ ...this.modelSignal() });
      } finally {
        this.isLoading.set(false)
      }
    })
  }

  onCancel(): void {
    this.cancel.emit();
  }

  protected readonly fieldErrors = createFieldErrors({
    name: () => this.categoryForm.name(),
    transactionType: () => this.categoryForm.transactionType()
  })
}
