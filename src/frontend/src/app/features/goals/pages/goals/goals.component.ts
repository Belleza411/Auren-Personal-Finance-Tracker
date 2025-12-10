import { Component, DestroyRef, inject, OnInit, signal } from "@angular/core";
import { GoalService } from "../../services/goal.service";
import { Goal, GoalsSummary } from "../../models/goals.model";
import { finalize, forkJoin } from "rxjs";
import { takeUntilDestroyed } from "@angular/core/rxjs-interop";


@Component({
  selector: 'app-goals',
  imports: [],
  templateUrl: './goals.component.html',
  styleUrl: './goals.component.css',
})
export class GoalsComponent implements OnInit {
  private goalSer = inject(GoalService);
  private destroyRef = inject(DestroyRef);
  
  goals = signal<Goal[]>([]);
  goalsSummary = signal<GoalsSummary | null>(null);
  
  isLoading = signal(false);
  error = signal<string | null>(null);

  ngOnInit(): void {
    this.loadData();
  }

  private loadData() {
    this.isLoading.set(true);
    this.error.set(null);

    forkJoin({
      goals: this.goalSer.getAllGoals(),
      goalsSummary: this.goalSer.getGoalsSummary()
    })
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        finalize(() => this.isLoading.set(false))
      )
      .subscribe({
        next: ({ goals, goalsSummary }) => {
          this.goals.set(goals),
          this.goalsSummary.set(goalsSummary)
        },
        error: err => {
          console.error("Failed to load data: ", err);
          this.error.set("Failed to load data. Please try again.");
        }
      })
  }

  deleteGoal(id: string) {
    this.goalSer.deleteGoal(id)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.goals.update(list => list.filter(g => g.goalId !== id)),
          this.loadData()
        },
        error: err => {
          console.error('Failed to delete goal:', err);
          this.error.set('Failed to delete goal. Please try again.');
        }
      })
  }
}
 