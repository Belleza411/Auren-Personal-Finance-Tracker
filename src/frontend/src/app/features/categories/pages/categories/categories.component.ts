import { Component, DestroyRef, inject, OnInit, signal } from '@angular/core';
import { CategoryService } from '../../services/category.service';
import { Category, CategoryOverview, CategorySummary } from '../../models/categories.model';
import { finalize, forkJoin } from 'rxjs';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';

@Component({
  selector: 'app-categories',
  imports: [],
  templateUrl: './categories.component.html',
  styleUrl: './categories.component.css',
})
export class CategoriesComponent implements OnInit {
  private readonly categorySer = inject(CategoryService);
  private destroyRef = inject(DestroyRef);
  
  categories = signal<Category[]>([]);
  categoriesOverview = signal<CategoryOverview[]>([]);
  categorySummary = signal<CategorySummary | null>(null);
  isLoading = signal(false);
  error = signal<string | null>(null);

  ngOnInit(): void {
    this.loadData();
  }

  private loadData() {
    this.isLoading.set(true);
    this.error.set(null);

    forkJoin({
      categories: this.categorySer.getAllCategories(),
      categoriesOverview: this.categorySer.getCategoriesOverview(),
      categorySummary: this.categorySer.getCategorySummary()
    })
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        finalize(() => this.isLoading.set(false))
      )
      .subscribe({
          next: ({ categories, categoriesOverview, categorySummary }) => {
            this.categories.set(categories),
            this.categoriesOverview.set(categoriesOverview),
            this.categorySummary.set(categorySummary)
          },
          error: err => {
            console.error("Failed to load data: ", err);
            this.error.set("Failed to load data. Please try again.");
          }
      });
  }

  deleteCategory(id: string) {
    this.categorySer.deleteCategory(id)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.categories.update(list => list.filter(c => c.categoryId !== id));
          this.loadData();
        }
      })
  }
}
