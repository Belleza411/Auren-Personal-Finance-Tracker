import { Component, computed, DestroyRef, inject, OnInit, resource, signal } from "@angular/core";
import { GoalService } from "../../services/goal.service";
import { Goal, GoalFilter, GoalsSummary, NewGoal } from "../../models/goals.model";
import { filter, finalize, firstValueFrom, forkJoin, switchMap, tap } from "rxjs";
import { takeUntilDestroyed } from "@angular/core/rxjs-interop";
import { ActivatedRoute, Router } from "@angular/router";
import { MatDialog } from "@angular/material/dialog";
import { AddGoal } from "../../components/add-goal/add-goal";
import { EditGoal } from "../../components/edit-goal/edit-goal";


@Component({
  selector: 'app-goals',
  imports: [],
  templateUrl: './goals.component.html',
  styleUrl: './goals.component.css',
})
export class GoalsComponent implements OnInit  {
  private goalSer = inject(GoalService);
  private destroyRef = inject(DestroyRef);
  private router = inject(Router);
  private route = inject(ActivatedRoute);
  private dialog = inject(MatDialog);

  goals = computed(() => this.goalResource.value()?.items ?? [])
  isLoading = computed(() => this.goalResource.isLoading());
  totalCount = computed(() => this.goalResource.value()?.totalCount)

  pageNumber = signal<number>(1);
  pageSize = signal<number>(10);

  currentFilters = signal<GoalFilter>({
    searchTerm: '',
    status: null,
    minBudget: null,
    maxBudget: null,
    targetFrom: null,
    targetTo: null
  })

  ngOnInit(): void {
    this.route.params
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(params => {
        const goalId = params['id'];
        const shouldOpenEditModal = this.route.snapshot.data['openEditModal'];
        const shouldOpenAddModal = this.route.snapshot.data['openAddModal'];

        if(goalId && shouldOpenEditModal) {
          this.openEditModalById(goalId);
        } else if (shouldOpenAddModal) {
          this.openAddModal();
        }
      })
  }

  goalResource = resource({
    params: () => ({
      filters: this.currentFilters(),
      pageSize: this.pageSize(),
      pageNumber: this.pageNumber()
    }),
    loader: ({ params }) => {
      return firstValueFrom(this.goalSer.getAllGoals(
        params.filters,
        params.pageNumber,
        params.pageSize
      ));
    }
  })
  
  deleteGoal(id: string) {
    this.goalSer.deleteGoal(id)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => this.goalResource.reload(),
        error: err => console.error('Failed to delete goal:', err)
      })
  }

  openEditModalById(id: string) {
    const goal = this.goals()
      .find(g => g.goalId === id);

    if(!goal) {
      console.error('Goal not found');
      this.router.navigate(['/goals']);
      return;
    }

    this.openEditModal(goal);
  }

  openAddModal() {
    const dialogRef = this.dialog.open<
      AddGoal,
      Goal[],
      NewGoal
    >(AddGoal, {
      width: '30rem',
      height: '100%',
      position: {
          top: '0',
          bottom: '0',
          right: '0'
      },
      panelClass: 'dialog',
      data: this.goals()
    });

    dialogRef.afterClosed()
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        tap(() => this.router.navigate(['/goals'])),
        filter((result): result is NewGoal => !!result),
        switchMap(result => this.goalSer.createGoal(result))
      )
      .subscribe({
        next: () => this.goalResource.reload(),
        error: err => console.error('Created failed: ', err)     
      })
  }

  openEditModal(goal: Goal) {
    const dialogRef = this.dialog.open<
      EditGoal,
      Goal,
      NewGoal>
    (EditGoal, {
      width: '30rem',
      height: '100%',
      position: {
        top: '0',
        bottom: '0',
        right: '0'
      },
      data: goal,
      panelClass: 'dialog',
      disableClose: false
    });

    dialogRef.afterClosed()
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        tap(() => this.router.navigate(['/goals'])),
        filter((result): result is NewGoal => !!result),
        switchMap(result => this.goalSer.updateGoal(goal.goalId, result))
      )
      .subscribe({
        next: () => this.goalResource.reload(),
        error: err => console.error('Update failed: ', err)     
      })
  }

  onEdit(id: string) {
    this.router.navigate(['/goals', id, 'edit']);
  }

  onAddGoal() {
    this.router.navigate(['/goals', 'create']);
  }

  onFiltersChange(filters: GoalFilter) {
    if (JSON.stringify(filters) === JSON.stringify(this.currentFilters())) {
      return;
    }

    this.currentFilters.set(filters);
    this.pageNumber.set(1);
  }

  onPageChange(page: number): void {
    this.pageNumber.set(page);
  }

  onPageSizeChange(size: number): void {
    this.pageSize.set(size);
    this.pageNumber.set(1);
  }
}
 