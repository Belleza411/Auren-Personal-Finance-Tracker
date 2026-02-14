import { ChangeDetectionStrategy, Component, computed, DestroyRef, inject, OnInit, resource, signal } from "@angular/core";
import { GoalService } from "../../services/goal.service";
import { Goal, GoalFilter, GoalStatus, NewGoal } from "../../models/goals.model";
import { filter, firstValueFrom, switchMap, tap } from "rxjs";
import { takeUntilDestroyed } from "@angular/core/rxjs-interop";
import { ActivatedRoute, Router } from "@angular/router";
import { MatDialog } from "@angular/material/dialog";
import { AddGoal } from "../../components/add-goal/add-goal";
import { EditGoal } from "../../components/edit-goal/edit-goal";
import { CurrencyPipe } from "@angular/common";
import { AddMoneyForm } from "../../components/add-money-form/add-money-form";
import { PaginationComponent } from "../../../../shared/components/pagination/pagination";
import { generateBgColorByEmoji } from "../../utils/generateBgColorByEmoji";

@Component({
  selector: 'app-goals',
  imports: [CurrencyPipe, PaginationComponent],
  templateUrl: './goals.component.html',
  styleUrl: './goals.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class GoalsComponent implements OnInit  {
  private goalSer = inject(GoalService);
  private destroyRef = inject(DestroyRef);
  private router = inject(Router);
  private route = inject(ActivatedRoute);
  private dialog = inject(MatDialog);

  dummyGoals = signal<Goal[]>([
    {
      goalId: 'goal-001',
      userId: 'user-123',
      name: 'Buy a New Laptop',
      description: 'Save money to buy a high-performance laptop for development and studies.',
      emoji: '💻',
      spent: 450,
      budget: 1500,
      goalStatus: 2,
      completionPercentage: 30,
      timeRemaining: '3 months',
      createdAt: 'January 5, 2025',
      targetDate: 'April 30, 2025',
    },
    {
      goalId: 'goal-002',
      userId: 'user-123',
      name: 'Emergency Savings Fund',
      description: 'Build an emergency fund for unexpected expenses.',
      emoji: '💰',
      spent: 1200,
      budget: 3000,
      goalStatus: 2,
      completionPercentage: 40,
      timeRemaining: '6 months',
      createdAt: 'December 1, 2024',
      targetDate: 'August 1, 2025',
    },
    {
      goalId: 'goal-003',
      userId: 'user-123',
      name: 'Vacation Trip',
      description: 'Save for a short holiday trip with friends.',
      emoji: '🌴',
      spent: null,
      budget: 2000,
      goalStatus: 4,
      completionPercentage: null,
      timeRemaining: '9 months',
      createdAt: 'January 20, 2025',
      targetDate: 'October 15, 2025',
    }
  ])

  pageNumber = signal<number>(1);
  pageSize = signal<number>(3);

  goals = computed(() => this.goalResource.value()?.items ?? this.dummyGoals())
  // isLoading = computed(() => this.goalResource.isLoading());
  isLoading = signal(false);
  totalCount = computed(() => this.goalResource.value()?.totalCount ?? 100);

  goalsWithColor = computed(() =>
    this.goals().map(goal => ({
      ...goal,
      bgColor: generateBgColorByEmoji(goal.emoji)
    }))
  );
  
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
    this.route.params
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(params => {
        const goalId = params['id'];
        const shouldOpenEditModal = this.route.snapshot.data['openEditModal'];
        const shouldOpenAddModal = this.route.snapshot.data['openAddModal'];
        const shouldOpenAddMoneyModal = this.route.snapshot.data['openAddMoneyModal'];

        if(goalId && shouldOpenEditModal) {
          this.openEditModalById(goalId);
        } else if (shouldOpenAddModal) {
          this.openAddModal();
        } else if(goalId && shouldOpenAddMoneyModal) {
          this.openAddMoneyModal(goalId);
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

  openAddMoneyModal(id: string) {
    const dialogRef = this.dialog.open<AddMoneyForm, null, number>(AddMoneyForm, {
      width: '30rem'
    })

    dialogRef.afterClosed()
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        tap(() => this.router.navigate(['/goals'])),
        filter(Boolean),
        switchMap(result => this.goalSer.addMoneyToGoal(result, id)) 
      )
      .subscribe({
        next: value => this.goalResource.reload(),
        error: err => console.error('Adding money failed: ', err) 
      })
  }

  onEdit(id: string) {
    this.router.navigate(['/goals', id, 'edit']);
  }

  onAddMoney(id: string) {
    this.router.navigate(['/goals', id, 'add-money']);
  }

  onAddGoal() {
    this.router.navigate(['/goals', 'create']);
  }

  onStatusChange(goal: Goal, event: Event) {
    const value = Number((event.target as HTMLSelectElement).value);
    if (value === goal.goalStatus) return;

    this.goalSer.updateGoal(goal.goalId, { status: value })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => this.goalResource.reload(),
        error: err => console.error('Failed to update status:', err)
      })
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

  isShowPagination(): boolean {
    return this.goals().length >= 3;
  }
}
 