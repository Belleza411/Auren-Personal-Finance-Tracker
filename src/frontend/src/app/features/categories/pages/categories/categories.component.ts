import { ChangeDetectionStrategy, Component, computed, DestroyRef, inject, input, signal } from '@angular/core';
import { 
  debounceTime,
  distinctUntilChanged,
  filter,
  startWith,
  switchMap,
  take,
  tap
} from 'rxjs';
import { rxResource, takeUntilDestroyed, toObservable, toSignal } from '@angular/core/rxjs-interop';
import { ActivatedRoute, NavigationEnd, Router } from '@angular/router';
import { MatDialog } from '@angular/material/dialog';
import { NoopScrollStrategy } from '@angular/cdk/overlay';

import { Category, CategoryFilter, NewCategory } from '../../models/categories.model';
import { AddCategory } from '../../components/add-category/add-category';
import { EditCategory } from '../../components/edit-category/edit-category';
import { CategoryTable } from "../../components/category-table/category-table";
import { CategoryStateService } from '../../services/category-state.service';
import { FilterKindConfig } from '../../../../shared/ui/filters/models/filter.model';
import { CATEGORY_FILTER_KIND_CONFIG } from '../../../../shared/constants/type-options';
import { Filter } from "../../../../shared/ui/filters/filter/filter";
import { PaginationComponent } from "../../../../shared/ui/pagination/pagination";
import { TimePeriod } from '../../../../core/models/time-period.enum';
import { AlertService } from 'src/app/core/services/alert.service';

@Component({
  selector: 'app-categories',
  imports: [CategoryTable, Filter, PaginationComponent],
  templateUrl: './categories.component.html',
  styleUrl: './categories.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class CategoriesComponent {
  private categoryStateSer = inject(CategoryStateService);
  private alert = inject(AlertService);
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

  private debouncedFilters = toSignal(
      toObservable(this.rawFilters).pipe(
          debounceTime(300),
          distinctUntilChanged((a, b) =>
              JSON.stringify(a, Object.keys(a).sort()) === JSON.stringify(b, Object.keys(b).sort())
          )
      ),
      { initialValue: this.rawFilters() }
  );

  private reloadTrigger = signal(0)

  private categoryResource = rxResource({
    params: () => ({
      filters: this.debouncedFilters(),
      pageSize: this.pagination().pageSize,
      pageNumber: this.pagination().pageNumber,
      reload: this.reloadTrigger()
    }),
    stream: ({ params }) =>
      this.categoryStateSer.getCategories(params.filters, params.pageSize, params.pageNumber)
  })

  categories = computed(() => this.categoryResource.value()?.items ?? [])
  totalCount = computed(() => this.categoryResource.value()?.totalCount ?? 0);
  isLoading = computed(() => this.categoryResource.isLoading());
  pageSize = computed(() => this.pagination().pageSize);
  pageNumber = computed(() => this.pagination().pageNumber);

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
    this.router.events.pipe(
      filter(e => e instanceof NavigationEnd),
      startWith(null),
      takeUntilDestroyed(this.destroyRef)
    ).subscribe(() => this.tryOpenDialog());

    if(!this.isLoading()) return;
  }

  private tryOpenDialog(): void {
    if (this.dialog.openDialogs.length > 0) return;
    if(this.isLoading()) return;

    const childRoute = this.route.firstChild;
    if (!childRoute) return;

    const { openAddModal, openEditModal } = childRoute.snapshot.data;
    const id = childRoute.snapshot.paramMap.get('id');

    if(openAddModal) {
      this.openAddModal();
      return;
    }

    const category = this.categories().find(c => c.id === id);

    if(openEditModal && id) {
      if (!category) return;
      
      this.openEditModal(category);
    }
  }

  deleteCategory(id: string) {
    const category = this.categories().find(c => c.id === id);
    if (!category) {
      this.alert.error('Category not found', "The category you're trying to delete does not exist.");
      return;
    }

    this.categoryStateSer.deleteCategory(id)
      .pipe(
        take(1),
        takeUntilDestroyed(this.destroyRef),
        tap(() => this.categoryResource.reload())
      )
      .subscribe({
        next: () => this.alert.success('Category Deleted', `The category has been deleted successfully.`),
        error: () => this.alert.error('Failed to delete category', `This category could not be deleted. Please try again later.`)
      })
  }

  openAddModal(): void {
    const dialogRef = this.dialog.open<
      AddCategory, 
      NewCategory>(
        AddCategory,
        {
          scrollStrategy: new NoopScrollStrategy(),
          width: '30rem'
        }
    );

    dialogRef.afterClosed()
      .pipe(
        take(1),
        takeUntilDestroyed(this.destroyRef),
        tap(() => this.router.navigate(['/categories'])),
        filter((result): result is NewCategory => !!result),
        switchMap(result => this.categoryStateSer.createCategory(result))
      )
      .subscribe({
        next: result => {
          this.categoryStateSer.clearCache();
          this.categoryResource.reload();
          this.alert.success('Category Added', `The category has been added successfully.`);
        },
        error: () => this.alert.error('Failed to add category', "A category with this name may already exist.")
      })
  }

  openEditModal(category: Category) {
    const dialogRef = this.dialog.open<
      EditCategory,
      Category,
      NewCategory>(EditCategory, {
        scrollStrategy: new NoopScrollStrategy(),
        width: '30rem',
        data: category
      })

    dialogRef.afterClosed()
      .pipe(
        take(1),
        takeUntilDestroyed(this.destroyRef),
        tap(() => this.router.navigate(['/categories'])),
        filter((result): result is NewCategory => !!result),
        switchMap(result => 
          this.categoryStateSer.updateCategory(category.id, result)
        )
      )
      .subscribe({
        next: result => {
          this.categoryStateSer.clearCache();
          this.categoryResource.reload();
          this.alert.success('Category Updated', `The category has been updated successfully.`);
        },
        error: () => this.alert.error('Failed to update category', "Unable to update category.")
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
