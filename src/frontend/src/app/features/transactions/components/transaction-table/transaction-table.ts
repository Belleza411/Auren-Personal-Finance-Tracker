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
export class TransactionTable {
  transactions = input.required<Transaction[]>();
  categories = input.required<Category[]>();
  
  delete = output<string>();
  edit = output<string>(); 

  onDelete(id: string) {
    this.delete.emit(id);
  }

  onEdit(id: string) {
    this.edit.emit(id);
  }

  protected TransactionTypeMap = TransactionTypeMap;
  protected PaymentTypeMap = PaymentTypeMap;
  protected categoryMap = computed(() => {
    return new Map(
      this.categories().map(c => [c.categoryId, c.name])
    );
  });
}
