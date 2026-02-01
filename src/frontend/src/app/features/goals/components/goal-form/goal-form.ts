import { ChangeDetectionStrategy, Component, computed, effect, input, output, signal } from '@angular/core';
import { NewGoal } from '../../models/goals.model';
import { FieldState, form, FormField, required, submit, validate } from '@angular/forms/signals';

@Component({
  selector: 'app-goal-form',
  imports: [FormField],
  templateUrl: './goal-form.html',
  styleUrl: './goal-form.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class GoalForm {
  isEdit = input(false);
  model = input.required<NewGoal>();
  save = output<NewGoal>();
  cancel = output<void>();

  isLoading = signal(false);

  goalStatusOptions: string[] = ["Completed", "On Track", "On Hold", "Not Started", "Behind Schedule", "Cancelled"];

  private greaterThanZero = (fieldName: string) => ({ value }: { value: () => number }) => {
    if (value() <= 0) {
      return {
        kind: 'invalidInput',
        message: `${fieldName} must be greater than 0`
      };
    }
    return null;
  };

  private futureDate = ({ value }: { value: () => string | Date }) => {
    const inputDate = new Date(value());
    const today = new Date();

    today.setHours(0, 0, 0, 0);

    if (inputDate <= today) {
      return {
        kind: 'invalidInput',
        message: 'Target date must be after today'
      };
    }
    return null;
  };

  private readonly modelSignal = signal({} as NewGoal);
  protected readonly goalForm = form(this.modelSignal, schema => {
    required(schema.name, { message: "Name is required"});
    required(schema.description, { message: "Description is required" });
    required(schema.budget, { message: "Budget is required" });
    required(schema.targetDate, { message: "Target date is required" });
    required(schema.status, { message: "Status is required "});

    validate(schema.spent, this.greaterThanZero('Spent'));
    validate(schema.budget, this.greaterThanZero('Budget'));
    validate(schema.targetDate, this.futureDate);
  });

  constructor() {
    effect(() => {
      const model = this.model();

      this.modelSignal.set({
        ...model,
        status: this.isEdit() ? model.status : null
      });
    });
  }

  onSubmit(event: Event) {
    event.preventDefault();

    submit(this.goalForm, async () => {
      this.isLoading.set(true);
      this.save.emit(this.modelSignal());
    })
  }

  onCancel(): void {
    this.cancel.emit();
  }

  protected readonly fieldErrors = {
    name: this.createErrorSignal(() => this.goalForm.name()),
    description: this.createErrorSignal(() => this.goalForm.description()),
    spent: this.createErrorSignal(() => this.goalForm.spent()),
    budget: this.createErrorSignal(() => this.goalForm.budget()),
    status: this.createErrorSignal(() => this.goalForm.status()),
    targetDate: this.createErrorSignal(() => this.goalForm.targetDate())
  }

  private createErrorSignal<T>(field: () => FieldState<T>) {
    return computed(() => this.setShowError(field()));
  };

  private setShowError<T>(field: FieldState<T>) {
    return field.invalid() && field.touched();
  };
}
