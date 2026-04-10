import { Component, computed, DestroyRef, effect, inject, input, OnInit, resource, signal } from '@angular/core';
import { CategoryService } from '../../services/category.service';
import { Category, CategoryFilter, NewCategory } from '../../models/categories.model';
import { combineLatest, debounceTime, distinctUntilChanged, filter, finalize, firstValueFrom, shareReplay, startWith, Subject, switchMap, take, tap } from 'rxjs';
import { takeUntilDestroyed, toObservable, toSignal } from '@angular/core/rxjs-interop';
import { ActivatedRoute, Router } from '@angular/router';
import { MatDialog } from '@angular/material/dialog';
import { AddCategory } from '../../components/add-category/add-category';
import { EditCategory } from '../../components/edit-category/edit-category';
import { CategoryTable } from "../../components/category-table/category-table";
import { CategoryStateService } from '../../services/category-state.service';
import { FilterKindConfig } from '../../../../shared/ui/filters/models/filter.model';
import { CATEGORY_FILTER_KIND_CONFIG } from '../../../../shared/constants/type-options';
import { Filter } from "../../../../shared/ui/filters/filter/filter";
import { PaginationComponent } from "../../../../shared/ui/pagination/pagination";
import { TimePeriod } from '../../../../core/models/time-period.enum';

@Component({
  selector: 'app-categories',
  imports: [CategoryTable, Filter, PaginationComponent],
  templateUrl: './categories.component.html',
  styleUrl: './categories.component.css',
})
export class CategoriesComponent {
  private readonly categorySer = inject(CategoryService);
  private categoryStateSer = inject(CategoryStateService);
  private destroyRef = inject(DestroyRef);
  private router = inject(Router);
  private route = inject(ActivatedRoute);
  private dialog = inject(MatDialog);

  selectedRange = signal<TimePeriod>(1);
  timePeriodOptions: string[] = ['All Time', 'This Month', 'Last Month', 'Last 3 Months', 'Last 6 Months', 'This Year'];

  rawFilters = signal<CategoryFilter>({
    searchTerm: '',
    transactionType: null,
  });

  id = input<string | null>(null);
  private pagination = signal({ pageNumber: 1, pageSize: 10});
  config = signal<FilterKindConfig<CategoryFilter>[]>(CATEGORY_FILTER_KIND_CONFIG);
  pageSizeOptions: number[] = [10, 20, 30, 40, 50];

  private debouncedFilter$ = toObservable(this.rawFilters).pipe(
    debounceTime(300),
    distinctUntilChanged((a, b) =>
      JSON.stringify(a, Object.keys(a).sort()) === JSON.stringify(b, Object.keys(b).sort())
    )
  );
  private pageNumber$ = toObservable(computed(() => this.pagination().pageNumber));
  private pageSize$ = toObservable(computed(() => this.pagination().pageSize));
  private reload$ = new Subject<void>();

  private categoryData$ = combineLatest([
    this.debouncedFilter$,
    this.pageNumber$,
    this.pageSize$,
    this.reload$.pipe(startWith(null))
  ]).pipe(
    switchMap(([filters, pageNumber, pageSize]) =>
      this.categoryStateSer.getCategories(filters, pageSize, pageNumber).pipe(
        startWith(null)
      )
    ),
    shareReplay(1)
  )

  categoryData = toSignal(this.categoryData$, { initialValue: null });
  categories = computed(() => this.categoryData()?.items ?? [])
  totalCount = computed(() => this.categoryData()?.totalCount ?? 0);
  isLoading = computed(() => this.categoryData() === null);

  pageSize = toSignal(this.pageSize$, { initialValue: 10 })
  pageNumber = toSignal(this.pageNumber$, { initialValue: 1 })

  selectedCategory = computed(() => 
    this.categories().find(c => c.id === this.id())
  );

  hasActiveFilters = computed(() => {
    const f = this.rawFilters();

    return (
        f.searchTerm !== '' ||
        f.transactionType !== null
    );
  })

  constructor() {
    effect(() => {
      const id = this.id();
      const category = this.selectedCategory();

      const { openEditModal, openAddModal } = this.route.snapshot.data;

      if (this.dialog.openDialogs.length > 0) return;

      if(this.isLoading()) return;

      if(id && openEditModal && category) {
        this.openEditModal(category)
        return;
      }

      if(id && openEditModal && !category) {
        this.router.navigate(['/categories']);
        return;
      }

      if(openAddModal) 
        this.openAddModal()
    })
  }

  deleteCategory(id: string) {
    this.categoryStateSer.deleteCategory(id)
      .pipe(
        take(1),
        takeUntilDestroyed(this.destroyRef),
        tap(() => this.reload$.next())
      )
      .subscribe()
  }

  openAddModal(): void {
    const dialogRef = this.dialog.open<
      AddCategory, 
      Category[], 
      NewCategory>(AddCategory, {
        width: '30rem',
        data: this.categories()
      });

    dialogRef.afterClosed()
      .pipe(
        take(1),
        takeUntilDestroyed(this.destroyRef),
        tap(() => this.router.navigate(['/categories'])),
        filter((result): result is NewCategory => !!result),
        switchMap(result => this.categorySer.createCategory(result))
      )
      .subscribe({
        next: () => {
          this.categoryStateSer.clearCache();
          this.reload$.next()
        },
        error: err => console.error('Created failed: ', err)
      })
  }

  openEditModal(category: Category) {
    const dialogRef = this.dialog.open<
      EditCategory,
      Category,
      NewCategory>(EditCategory, {
        width: '30rem',
        data: category
      })

    dialogRef.afterClosed()
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        tap(() => this.router.navigate(['/categories'])),
        filter((result): result is NewCategory => !!result),
        switchMap(result => 
          this.categoryStateSer.updateCategory(category.id, result)
        )
      )
      .subscribe({
        next: () => {
          this.categoryStateSer.clearCache();
          this.reload$.next()
        },
        error: err => console.error('Update failed: ', err)
      })
  }

  onEditFromTable(id: string): void {
    this.router.navigate(['/categories', id, 'edit']);
  }
 
  onAddCategory(): void {
    this.router.navigate(['/categories', 'create']);
  }

  onFiltersChange(filters: CategoryFilter) {
    this.rawFilters.set(filters);
    this.onPageChange(1);
  }

  onPageSizeChange(size: number) {
    this.pagination.set({ pageNumber: 1, pageSize: size }); 
  }

  onPageChange(page: number) {
    this.pagination.update(p => ({ ...p, pageNumber: page }));
  }

  onRangeChange(e: Event) {
    this.selectedRange.set(
      Number((e.target as HTMLSelectElement).value) + 1
    );
  }
}
