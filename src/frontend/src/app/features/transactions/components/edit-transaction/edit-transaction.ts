import { Component, DestroyRef, inject, OnInit, signal } from '@angular/core';
import { NewTransaction, Transaction } from '../../models/transaction.model';
import { MAT_DIALOG_DATA  } from '@angular/material/dialog';
import { DialogRef } from '@angular/cdk/dialog';
import { CategoryService } from '../../../categories/services/category.service';
import { Category } from '../../../categories/models/categories.model';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { TransactionForm } from "../transaction-form/transaction-form";

@Component({
  selector: 'app-edit-transaction',
  imports: [TransactionForm],
  templateUrl: './edit-transaction.html',
  styleUrl: './edit-transaction.css',
})
export class EditTransaction implements OnInit {
  protected readonly data = inject(MAT_DIALOG_DATA);
  private readonly categorySer = inject(CategoryService);
  protected dialogRef = inject(DialogRef<NewTransaction>);
  private destroyRef = inject(DestroyRef);

  private readonly transactionData: Transaction = this.data.transaction;
  protected readonly categoriesData: Category[] = this.data.categories;

  ngOnInit(): void {
    if(this.transactionData) {
      this.transactionModel.set({
        name: this.transactionData.name,
        amount: this.transactionData.amount,
        category: this.transactionData.categoryId,
        transactionType: this.transactionData.transactionType,
        paymentType: this.transactionData.paymentType
      });
    }
  }

  protected readonly transactionModel = signal<NewTransaction>({
    name: '',
    amount: 0,
    category: '',
    transactionType: 1,
    paymentType: 2
  })

  onSave(data: NewTransaction): void {
    this.dialogRef.close(data);
  }
}
