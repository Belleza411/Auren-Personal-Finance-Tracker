import { Component, DestroyRef, inject, signal } from '@angular/core';
import { NewTransaction } from '../../models/transaction.model';
import { DialogRef } from '@angular/cdk/dialog';
import { TransactionForm } from "../transaction-form/transaction-form";
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { CategoryService } from '../../../categories/services/category.service';
import { Category } from '../../../categories/models/categories.model';

@Component({
  selector: 'app-add-transaction',
  imports: [TransactionForm],
  templateUrl: './add-transaction.html',
  styleUrl: './add-transaction.css',
})
export class AddTransaction {
  protected dialogRef = inject(DialogRef<NewTransaction>);
  private destroyRef = inject(DestroyRef);
  private readonly categorySer = inject(CategoryService);
  
  categories = signal<Category[]>([
    {
      categoryId: '1',
      userId: '1',
      name: 'Salary',
      transactionType: 1,
      createdAt: 'June 1, 2025'
    },
    {
      categoryId: '2',
      userId: '1',
      name: 'Shopping',
      transactionType: 2,
      createdAt: "June 2, 2025"
    },
    {
      categoryId: '3',
      userId: '1',
      name: 'Health',
      transactionType: 2,
      createdAt: "June 10, 2025"
    }
  ]);

  ngOnInit(): void {
    this.categorySer.getAllCategories()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: data => this.categories.set(data),
        error: err => console.error(err)
      })
  }

  protected model = signal<NewTransaction>({
    name: '',
    amount: 0,
    category: '',
    transactionType: 1,
    paymentType: 2
  });

  onSave(data: NewTransaction) {
    this.dialogRef.close(data);
  }
}
