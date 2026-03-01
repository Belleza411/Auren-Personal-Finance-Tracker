import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { NewTransaction } from '../../models/transaction.model';
import { TransactionForm } from "../transaction-form/transaction-form";
import { Category } from '../../../categories/models/categories.model';
import { MAT_DIALOG_DATA  } from '@angular/material/dialog';
import { MatDialogRef } from '@angular/material/dialog';

@Component({
  selector: 'app-add-transaction',
  imports: [TransactionForm],
  templateUrl: './add-transaction.html',
  styleUrl: './add-transaction.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class AddTransaction {
  protected readonly data: Category[] = inject(MAT_DIALOG_DATA);
  protected dialogRef = inject(MatDialogRef<NewTransaction>);

  protected model = signal<NewTransaction>({
    name: '',
    amount: 0,
    category: '',
    transactionType: "Income",
    paymentType: "CreditCard",
    transactionDate: ''
  });

  onSave(data: NewTransaction) {
    console.log(data);
    this.dialogRef.close(data);
  }
}
