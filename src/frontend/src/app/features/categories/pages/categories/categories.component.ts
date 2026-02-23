import { Component, computed, DestroyRef, inject, OnInit, resource, signal } from '@angular/core';
import { CategoryService } from '../../services/category.service';
import { Category, CategoryFilter, NewCategory } from '../../models/categories.model';
import { filter, finalize, firstValueFrom, switchMap, tap } from 'rxjs';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ActivatedRoute, Router } from '@angular/router';
import { MatDialog } from '@angular/material/dialog';
import { TimePeriod } from '../../../transactions/models/transaction.model';
import { AddCategory } from '../../components/add-category/add-category';
import { EditCategory } from '../../components/edit-category/edit-category';
import { CategoryTable } from "../../components/category-table/category-table";
import { dummyCategories } from '../../../../shared/fake-data';

@Component({
  selector: 'app-categories',
  imports: [CategoryTable],
  templateUrl: './categories.component.html',
  styleUrl: './categories.component.css',
})
export class CategoriesComponent implements OnInit {
  private readonly categorySer = inject(CategoryService);
  private destroyRef = inject(DestroyRef);
  private router = inject(Router);
  private route = inject(ActivatedRoute);
  private dialog = inject(MatDialog);

  pageNumber = signal<number>(1);
  pageSize = signal<number>(10);

  selectedRange = signal<TimePeriod>(1);

  currentFilters = signal<CategoryFilter>({
    searchTerm: '',
    transactionType: null,
  });

  timePeriodOptions: string[] = ['All Time', 'This Month', 'Last Month', 'Last 3 Months', 'Last 6 Months', 'This Year'];

  protected readonly dummyCategories = signal<Category[]>(dummyCategories);

  categories = computed(() => this.categoryResource.value()?.items ?? []);
  isLoading = computed(() => this.categoryResource.isLoading())
  totalCount = computed(() => this.categoryResource.value()?.totalCount ?? 0);

  hasActiveFilters = computed(() => {
    const hasSearch = this.currentFilters().searchTerm.trim().length !== 0;
    const hasType = this.currentFilters().transactionType !== null;

    return hasSearch || hasType;
  })

  hasNoCategories = computed(() =>
    !this.isLoading() &&
    this.totalCount() === 0 &&
    !this.hasActiveFilters()
  )

  hasNoFilterResults = computed(() =>
      !this.isLoading() &&
      this.totalCount() === 0 &&
      this.hasActiveFilters()
  )

  ngOnInit(): void {
    this.route.params
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(params => {
        const categoryId = params['id'];
        const shouldOpenEditModal = this.route.snapshot.data['openEditModal'];
        const shouldOpenAddModal = this.route.snapshot.data['openAddModal'];

        if(categoryId && shouldOpenEditModal) {
          this.openEditModalById(categoryId)
        } else if (shouldOpenAddModal) {
          this.openAddModal();
        }
      })
  }

  categoryResource = resource({
    params: () => ({
      filters: this.currentFilters(),
      pageSize: this.pageSize(),
      pageNumber: this.pageNumber()
    }),
    loader: async ({ params }) => {
      return firstValueFrom(this.categorySer.getAllCategories(params.filters, 50, 1))
    }
  })

  deleteCategory(id: string) {
    this.categorySer.deleteCategory(id)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => this.categoryResource.reload()
      })
  }

  openEditModalById(id: string): void {
    const category = this.categories()
      .find(c => c.id === id);

    if(!category) {
      console.error('Category not found');
      this.router.navigate(['/categories'])
      return 
    }

    this.openEditModal(category);
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
        takeUntilDestroyed(this.destroyRef),
        tap(() => this.router.navigate(['/categories'])),
        filter((result): result is NewCategory => !!result),
        switchMap(result => this.categorySer.createCategory(result))
      )
      .subscribe({
        next: () => this.categoryResource.reload(),
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
          this.categorySer.updateCategory(
            category.id,
            result
          )
        )
      )
      .subscribe({
        next: () => this.categoryResource.reload(),
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
    if(JSON.stringify(filters) === JSON.stringify(this.currentFilters())) {
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

  onRangeChange(e: Event) {
    this.selectedRange.set(
      Number((e.target as HTMLSelectElement).value) + 1
    );
  }
}
