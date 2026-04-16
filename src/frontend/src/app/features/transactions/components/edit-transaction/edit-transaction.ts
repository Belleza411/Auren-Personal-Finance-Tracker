import { ChangeDetectionStrategy, Component, inject, OnInit, signal } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef  } from '@angular/material/dialog';

import { NewTransaction, Transaction } from '../../models/transaction.model';
import { Category, NewCategory } from '../../../categories/models/categories.model';
import { TransactionForm } from "../transaction-form/transaction-form";

@Component({
  selector: 'app-edit-transaction',
  imports: [TransactionForm],
  templateUrl: './edit-transaction.html',
  styleUrls: ['./edit-transaction.css', '../../styles/dialog-animation.css'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class EditTransaction implements OnInit {
  protected readonly data = inject(MAT_DIALOG_DATA);
  protected dialogRef = inject(MatDialogRef<NewTransaction>);

  isClosing = signal(false);
  private readonly closeResult = signal<NewTransaction | null>(null);

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
    transactionType: "Income",
    paymentType: "CreditCard",
    transactionDate: ''
  })

  onSave(data: NewTransaction): void {
    this.startClose(data);
  }

  startClose(result?: NewTransaction) {
    this.closeResult.set(result ?? null);
    this.isClosing.set(true);

    setTimeout(() => {
      this.dialogRef.close(this.closeResult())
    }, 200);
  }
}
