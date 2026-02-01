import { DialogRef } from '@angular/cdk/dialog';
import { ChangeDetectionStrategy, Component, inject, OnInit, signal } from '@angular/core';
import { MAT_DIALOG_DATA } from '@angular/material/dialog';
import { Goal, NewGoal } from '../../models/goals.model';
import { GoalForm } from "../goal-form/goal-form";

@Component({
  selector: 'app-edit-goal',
  imports: [GoalForm],
  templateUrl: './edit-goal.html',
  styleUrl: './edit-goal.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class EditGoal implements OnInit {
  protected readonly data: Goal = inject(MAT_DIALOG_DATA);
  protected dialogRef = inject(DialogRef<NewGoal>);

  ngOnInit(): void {
    if(this.data) {
      this.model.set({
        name: this.data.name,
        description: this.data.description,
        spent: this.data.spent ?? 0,
        budget: this.data.budget,
        status: this.data.goalStatus,
        targetDate: this.data.targetDate,
      })
    }
  }

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
