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
  styleUrls: ['./add-transaction.css','../../styles/dialog-animation.css'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class AddTransaction {
  protected readonly data: Category[] = inject(MAT_DIALOG_DATA);
  protected dialogRef = inject(MatDialogRef<NewTransaction>);

  isClosing = signal(false);
  private readonly closeResult = signal<NewTransaction | null>(null);

  protected model = signal<NewTransaction>({
    name: '',
    amount: 0,
    category: '',
    transactionType: "Income",
    paymentType: "CreditCard",
    transactionDate: ''
  });

  onSave(data: NewTransaction) {
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
