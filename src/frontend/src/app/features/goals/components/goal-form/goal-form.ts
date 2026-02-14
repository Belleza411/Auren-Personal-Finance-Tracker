import { afterNextRender, ChangeDetectionStrategy, ChangeDetectorRef, Component, computed, effect, inject, input, OnInit, output, signal } from '@angular/core';
import { NewGoal } from '../../models/goals.model';
import { FieldState, form, FormField, required, submit, validate } from '@angular/forms/signals';
import { PickerComponent } from '@ctrl/ngx-emoji-mart';

@Component({
  selector: 'app-goal-form',
  imports: [FormField, PickerComponent],
  templateUrl: './goal-form.html',
  styleUrl: './goal-form.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class GoalForm implements OnInit {
  private cdr = inject(ChangeDetectorRef);

  isEdit = input(false);
  model = input.required<NewGoal>();
  save = output<NewGoal>();
  cancel = output<void>();

  isLoading = signal(false);

  goalStatusOptions: string[] = ["Completed", "On Track", "On Hold", "Not Started", "Behind Schedule", "Cancelled"];

  emoji = computed(() => this.goalForm.emoji().value() || "😀");

  isEmojiPicker = signal(false);
  showEmojiPicker() {
    this.isEmojiPicker.update(v => !v);
  }

  private greaterThanZero = (fieldName: string) => ({ value }: { value: () => number }) => {
    const v = value();
    if(v == null) return null;

    if (v <= 0) {
      return {
        kind: 'invalidInput',
        message: `${fieldName} must be greater than 0`
      };
    }
    return null;
  };

  private futureDate = ({ value }: { value: () => string | Date }) => {
    if(!value()) return null;

    const inputDate = new Date(value());

    if(isNaN(inputDate.getTime())) return null;

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

  private readonly modelSignal = signal<NewGoal>({
    name: '',
    description: '',
    emoji: '',
    spent: 0,
    budget: 0,
    status: null,
    targetDate: ''
  });

  protected readonly goalForm = form(this.modelSignal, schema => {
    required(schema.name, { message: "Name is required"});
    required(schema.description, { message: "Description is required" });
    required(schema.emoji, { message: "Emoji is required" });
    required(schema.budget, { message: "Budget is required" });
    required(schema.targetDate, { message: "Target date is required" });
    required(schema.status, { message: "Status is required "});

    validate(schema.budget, this.greaterThanZero('Budget'));
    validate(schema.targetDate, this.futureDate);
  });

  ngOnInit(): void {
    const model = this.model();

    this.modelSignal.set({
      ...model,
      status: this.isEdit() ? model.status : null,
    });

  }

  constructor() {
    afterNextRender(() => {
      const model = this.model();
      if(model.emoji) {
         this.goalForm.emoji().value.set(model.emoji);
      }
    });
  }


  onSubmit(event: Event) {
    event.preventDefault();
    
    submit(this.goalForm, async () => {
      this.isLoading.set(true);

      const value: NewGoal = {
        name: this.goalForm.name().value(),
        description: this.goalForm.description().value(),
        emoji: this.goalForm.emoji().value(),
        spent: this.goalForm.spent().value(),
        budget: this.goalForm.budget().value(),
        status: this.goalForm.status().value(),
        targetDate: this.goalForm.targetDate().value(),
      };

      this.save.emit(value);
      console.log('Form submitted with value:', value);
    })
  }

  onCancel(): void {
    this.cancel.emit();
  }

  protected readonly fieldErrors = {
    name: this.createErrorSignal(() => this.goalForm.name()),
    description: this.createErrorSignal(() => this.goalForm.description()),
    emoji: this.createErrorSignal(() => this.goalForm.emoji()),
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

  selectEmoji(event: any) {
    this.goalForm.emoji().value.set(event.emoji.native);  
    this.isEmojiPicker.set(false); 

    this.cdr.markForCheck();
  }
}
