import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { MAT_DIALOG_DATA } from '@angular/material/dialog';
import { Goal, NewGoal } from '../../models/goals.model';
import { DialogRef } from '@angular/cdk/dialog';
import { GoalForm } from "../goal-form/goal-form";

@Component({
  selector: 'app-add-goal',
  imports: [GoalForm],
  templateUrl: './add-goal.html',
  styleUrl: './add-goal.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class AddGoal {
  protected readonly data: Goal[] = inject(MAT_DIALOG_DATA);
  protected dialogRef = inject(DialogRef<NewGoal>);

  protected model = signal<NewGoal>({
    name: '',
    description: '',
    spent: 0,
    budget: 0,
    status: 4,
    targetDate: ''
  });

  onSave(data: NewGoal) {
    this.dialogRef.close(data);
  }
}
