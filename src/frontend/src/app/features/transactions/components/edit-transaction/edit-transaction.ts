import { Component, DestroyRef, inject, OnInit, signal } from '@angular/core';
import { NewTransaction, Transaction } from '../../models/transaction.model';
import { MAT_DIALOG_DATA  } from '@angular/material/dialog';
import { DialogRef } from '@angular/cdk/dialog';
import { Field, form  } from '@angular/forms/signals';
import { CategoryService } from '../../../categories/services/category.service';
import { Category } from '../../../categories/models/categories.model';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { EnumSelect } from "../../../../shared/components/enum-select/enum-select";
import { PaymentTypeMap, TransactionTypeMap } from '../../constants/transaction-map';

@Component({
  selector: 'app-edit-transaction',
  imports: [Field, EnumSelect],
  templateUrl: './edit-transaction.html',
  styleUrl: './edit-transaction.css',
})
export class EditTransaction implements OnInit {
  protected readonly data: Transaction = inject(MAT_DIALOG_DATA);
  private readonly categorySer = inject(CategoryService);
  protected dialogRef = inject(DialogRef<NewTransaction>);
  private destroyRef = inject(DestroyRef);
  
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
  
  isLoading = signal(false);
  TransactionTypeMap = TransactionTypeMap;
  PaymentTypeMap = PaymentTypeMap;

  ngOnInit(): void {
    if (!this.data) {
      this.dialogRef.close();
      return;
    }

    this.transactionModel.set({
      name: this.data.name,
      amount: this.data.amount,
      category: this.data.categoryId,
      transactionType: this.data.transactionType,
      paymentType: this.data.paymentType
    });

    this.categorySer.getAllCategories()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: data => this.categories.set(data),
        error: err => console.error(err)
      })
  }

  protected readonly transactionModel = signal<NewTransaction>({
    name: '',
    amount: 0,
    category: '',
    transactionType: 1,
    paymentType: 2
  })

  protected readonly transactionForm = form(this.transactionModel);

  onCancel(): void {
    this.dialogRef.close();
  }

  onSave(): void {
    if (this.transactionForm().valid()) {
      const formValue = this.transactionForm().value();
      const updatedDto: NewTransaction = {
        name: formValue.name,
        amount: formValue.amount,
        category: formValue.category,
        transactionType: formValue.transactionType,
        paymentType: formValue.paymentType
      };

      this.dialogRef.close(updatedDto);
    }
  }
}
