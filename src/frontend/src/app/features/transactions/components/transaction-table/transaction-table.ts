import { Component, computed, DestroyRef, inject, input, OnInit, output } from '@angular/core';
import { NewTransaction, Transaction } from '../../models/transaction.model';
import { Category } from '../../../categories/models/categories.model';
import { CurrencyPipe } from '@angular/common';
import { PaymentTypeMap, TransactionTypeMap } from '../../constants/transaction-map';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { MatDialog } from '@angular/material/dialog';
import { EditTransaction } from '../edit-transaction/edit-transaction';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { TransactionService } from '../../services/transaction.service';

@Component({
  selector: 'app-transaction-table',
  imports: [CurrencyPipe],
  templateUrl: './transaction-table.html',
  styleUrl: './transaction-table.css',
})
export class TransactionTable implements OnInit {
  private dialog = inject(MatDialog);
  private destroyRef = inject(DestroyRef);
  private transactionSer = inject(TransactionService);
  private router = inject(Router);
  private route = inject(ActivatedRoute);
  
  transactions = input.required<Transaction[]>();
  categories = input.required<Category[]>();
  
  delete = output<string>();
  
  ngOnInit(): void {
    this.route.params.subscribe(params => {
      const transactionId = params['id'];
      const shouldOpenModal = this.route.snapshot.data['openEditModal'];
      
      if (transactionId && shouldOpenModal) {
        this.openEditModalById(transactionId);
      }
    });
  }

  onDelete(id: string) {
    this.delete.emit(id);
  }

  protected TransactionTypeMap = TransactionTypeMap;
  protected PaymentTypeMap = PaymentTypeMap;
  protected categoryMap = computed(() => {
    return new Map(
      this.categories().map(c => [c.categoryId, c.name])
    );
  });

  openEditModalById(transactionId: string): void {
    this.transactionSer.getTransactionById(transactionId).subscribe({
      next: (transaction) => {
        this.openEditModal(transaction.transactionId);
      },
      error: (err) => {
        console.error('Transaction not found', err);
        this.router.navigate(['/transactions']); 
      }
    });
  }

  openEditModal(id: string) {
    const transactionToEdit = this.transactions().find(t => t.transactionId === id);
    const dialogRef = this.dialog.open<EditTransaction, Transaction, NewTransaction>(
      EditTransaction, 
      {
        width: '500px',
        data: transactionToEdit,
        disableClose: false
      }
    );

    dialogRef.afterClosed()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe((result: NewTransaction | undefined) => {
        if (result) {
          this.transactionSer.updateTransaction(id, result)
            .subscribe({
              next: (data) => console.log(data),
              error: err => console.error(err)
            });
        }
      });

    this.router.navigate(['/transactions'])
  }

  onEditClick(id: string): void {
    this.openEditModal(id);
  }
}
