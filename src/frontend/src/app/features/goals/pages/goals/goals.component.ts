import { Component, computed, DestroyRef, inject, OnInit, resource, signal } from "@angular/core";
import { GoalService } from "../../services/goal.service";
import { Goal, GoalFilter, GoalsSummary, GoalStatus, NewGoal } from "../../models/goals.model";
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

  searchTerm = signal<string>('');
  status = signal<GoalStatus | null>(null);
  minBudget = signal<number | null>(null);
  maxBudget = signal<number | null>(null);
  targetFrom = signal<string | null>(null);
  targetTo = signal<string | null>(null);

  private readonly currentFilters = signal<GoalFilter>({
    searchTerm: this.searchTerm(),
    status: this.status(),
    minBudget: this.minBudget(),
    maxBudget: this.maxBudget(),
    targetFrom: this.targetFrom(),
    targetTo: this.targetTo()
  });

  goalStatusOptions: string[] = ['All Status', 'Completed', 'On Track', 'On Hold', 'Not Started', 'Behind Schedule', 'Cancelled']

  modals = signal({
    budget: false,
    targetDate: false
  })

  toggleModal(modalName: 'budget' | 'targetDate') {
    this.modals.update(modals => ({
      ...modals,
      [modalName]: !modals[modalName]
    }))
  }

  ngOnInit(): void {
    console.log(this.currentFilters());

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

  onChangeStatus(e: Event) {
    const value = Number((e.target as HTMLSelectElement).value);
    this.status.set(value === 0 ? null : value);
  }

  hasActiveFilters = computed(() => {
    const hasSearch = this.searchTerm().trim().length !== 0;
    const hasStatus = this.status() !== null;
    const hasBudget = this.minBudget() !== null || this.maxBudget() !== null;
    const hasTargetDate = this.targetFrom() !== null || this.targetTo() !== null;

    return hasSearch || hasStatus || hasBudget || hasTargetDate;
  })

  clearFilter() {
    this.searchTerm.set('');
    this.status.set(null);
    this.minBudget.set(null);
    this.maxBudget.set(null);
    this.targetFrom.set(null);
    this.targetTo.set(null);
  }

  clearBudgetFilter() {
    this.minBudget.set(null);
    this.maxBudget.set(null);
  }

  clearTargetDateFilter() {
    this.targetFrom.set(null);
    this.targetTo.set(null);
  }

  formatDate(event: Event) {
    const value = (event.target as HTMLInputElement).value;
    const date = new Date(value);
    return date.toLocaleDateString('en-US', {
      month: 'short', 
      day: 'numeric', 
      year: 'numeric', 
    })
  }
}
 