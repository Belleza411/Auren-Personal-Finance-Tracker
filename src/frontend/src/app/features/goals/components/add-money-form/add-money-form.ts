import { DialogRef } from '@angular/cdk/dialog';
import { Component, inject, signal } from '@angular/core';
import { form, required, submit, validate, FormField } from '@angular/forms/signals';

@Component({
  selector: 'app-add-money-form',
  imports: [FormField],
  templateUrl: './add-money-form.html',
  styleUrl: './add-money-form.css',
})
export class AddMoneyForm {
  isLoading = signal(false);
  private dialogRef = inject(DialogRef<number>);

  protected readonly modelSignal = signal(0);
  protected readonly modelForm = form(this.modelSignal, schema => {
    required(schema, { message: "Amount is required" });
    validate(schema, ({ value }) => {
      if(value() <= 0) {
        return {
          kind: "invalidInput",
          message: "Amount must be greater than 0"
        }
      }

      return;
    })
  });

  onSubmit(e: Event) {
    e.preventDefault();

    submit(this.modelForm, async () => {
      this.isLoading.set(true);
      this.dialogRef.close(this.modelSignal())
    })
  }
}
