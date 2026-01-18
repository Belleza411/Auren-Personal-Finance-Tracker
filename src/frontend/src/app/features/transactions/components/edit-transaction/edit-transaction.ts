import { ChangeDetectionStrategy, Component, inject, OnInit, signal } from '@angular/core';
import { NewTransaction, Transaction } from '../../models/transaction.model';
import { MAT_DIALOG_DATA  } from '@angular/material/dialog';
import { DialogRef } from '@angular/cdk/dialog';
import { Category } from '../../../categories/models/categories.model';
import { TransactionForm } from "../transaction-form/transaction-form";

@Component({
  selector: 'app-edit-transaction',
  imports: [TransactionForm],
  templateUrl: './edit-transaction.html',
  styleUrl: './edit-transaction.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class EditTransaction implements OnInit {
  protected readonly data = inject(MAT_DIALOG_DATA);
  protected dialogRef = inject(DialogRef<NewTransaction>);

  private readonly transactionData: Transaction = this.data.transaction;
  protected readonly categoriesData: Category[] = this.data.categories;

  ngOnInit(): void {
    if(this.transactionData) {
      this.transactionModel.set({
        name: this.transactionData.name,
        amount: this.transactionData.amount,
        category: this.transactionData.categoryId,
        transactionType: this.transactionData.transactionType,
        paymentType: this.transactionData.paymentType,
        transactionDate: this.transactionData.transactionDate
      });
    }
  }

  protected readonly transactionModel = signal<NewTransaction>({
    name: '',
    amount: 0,
    category: '',
    transactionType: 1,
    paymentType: 2,
    transactionDate: ''
  })

  onSave(data: NewTransaction): void {
    this.dialogRef.close(data);
  }
}
