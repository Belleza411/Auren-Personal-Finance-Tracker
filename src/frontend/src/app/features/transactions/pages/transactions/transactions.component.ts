import { Component, computed, DestroyRef, inject, OnInit, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { filter, finalize, forkJoin, switchMap, tap } from 'rxjs';
import { ActivatedRoute, Router, RouterOutlet } from "@angular/router";
import { MatDialog } from '@angular/material/dialog';
import { CountUpDirective } from 'ngx-countup';
import { CurrencyPipe } from '@angular/common';

import { TransactionService } from '../../services/transaction.service';
import { NewTransaction, Transaction } from '../../models/transaction.model';
import { TransactionTable } from "../../components/transaction-table/transaction-table";
import { SummaryCard } from "../../../../shared/components/summary-card/summary-card";
import { Category } from '../../../categories/models/categories.model';
import { CategoryService } from '../../../categories/services/category.service';
import { EditTransaction } from '../../components/edit-transaction/edit-transaction';
import { AddTransaction } from '../../components/add-transaction/add-transaction';

@Component({
  selector: 'app-transaction',
  imports: [TransactionTable, SummaryCard, CurrencyPipe, RouterOutlet, CountUpDirective],
  templateUrl: './transactions.component.html',
  styleUrl: './transactions.component.css',
})
export class TransactionComponent implements OnInit {
    private transactionSer = inject(TransactionService);
    private categorySer = inject(CategoryService);
    private destroyRef = inject(DestroyRef);
    private router = inject(Router);
    private route = inject(ActivatedRoute);
    private dialog = inject(MatDialog);

    pageNumber = signal(1);
    pageSize = signal(5);

    transactions = signal<Transaction[]>(
    [
        {
            transactionId: '1',
            userId: '1',
            categoryId: '1',
            transactionType: 1,
            name: 'Freelance Payment',
            amount: 2000.00,
            paymentType: 3,
            transactionDate: "June 1, 2025",
            createdAt: "June 1, 2025"
        },
        {
            transactionId: '2',
            userId: '1',
            categoryId: '2',
            transactionType: 2,
            name: 'Groceries',
            amount: 100.00,
            paymentType: 3,
            transactionDate: "June 2, 2025",
            createdAt: "June 2, 2025"
        },
        {
            transactionId: '3',
            userId: '1',
            categoryId: '3',
            transactionType: 2,
            name: 'Health Insurance',
            amount: 55.00,
            paymentType: 3,
            transactionDate: "June 10, 2025",
            createdAt: "June 10, 2025"
        },
        {
            transactionId: '4',
            userId: '1',
            categoryId: '4',
            transactionType: 1,
            name: 'Gas',
            amount: 50.00,
            paymentType: 3,
            transactionDate: "June 5, 2025",
            createdAt: "June 5, 2025"
        },
        {
            transactionId: '5',
            userId: '1',
            categoryId: '1',
            transactionType: 1,
            name: 'Freelance Payment',
            amount: 1000.00,
            paymentType: 3,
            transactionDate: "June 11, 2025",
            createdAt: "June 11, 2025"
        },
        {
            transactionId: '6',
            userId: '1',
            categoryId: '1',
            transactionType: 1,
            name: 'Freelance Payment',
            amount: 1000.00,
            paymentType: 3,
            transactionDate: "June 15, 2025",
            createdAt: "June 15, 2025"
        },
        {
            transactionId: '7',
            userId: '1',
            categoryId: '1',
            transactionType: 1,
            name: 'Freelance Payment',
            amount: 400.00,
            paymentType: 3,
            transactionDate: "June 20, 2025",
            createdAt: "June 20, 2025"
        }
    ]);
    categories = signal<Category[]>([
        {
            categoryId: '1',
            userId: '1',
            name: 'Salary',
            transactionType: 1,
            createdAt: "June 1, 2025"
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

    avgDailySpending = signal<number>(500);
    totalBalance = signal<number>(2500);
    income = signal<number>(2000);
    expense = signal<number>(500);
    isLoading = signal(false);
    error = signal<string | null>(null);

    options = {
        duration: 1.2,
        separator: ',',
        prefix: '$'
    };
    
    ngOnInit(): void {
        this.loadData();

        this.route.params
            .pipe(takeUntilDestroyed(this.destroyRef))
            .subscribe(params => {
                const transactionId = params['id'];
                const shouldOpenEditModal = this.route.snapshot.data['openEditModal'];
                const shouldOpenAddModal = this.route.snapshot.data['openAddModal'];
                
                if (transactionId && shouldOpenEditModal) {
                    this.openEditModalById(transactionId);
                } else if (shouldOpenAddModal) {
                    this.openAddModal();
                }
            });
    }  

    private loadData() {
        this.isLoading.set(true);
        this.error.set(null);

        forkJoin({
            transactions: this.transactionSer.getAllTransactions({}, this.pageSize(), this.pageNumber()),
            avgDailySpending: this.transactionSer.getAvgDailySpending(),
            balance: this.transactionSer.getBalance(),
            categories: this.categorySer.getAllCategories()
        })
            .pipe(
                takeUntilDestroyed(this.destroyRef),
                finalize(() => this.isLoading.set(false))
            )
            .subscribe({
                next: ({ transactions, avgDailySpending, balance, categories }) => {
                    this.transactions.set(transactions.items);
                    this.avgDailySpending.set(avgDailySpending);
                    this.totalBalance.set(balance.balance);
                    this.income.set(balance.income);
                    this.expense.set(balance.expense);
                    this.categories.set(categories)
                },
                error: err => {
                    console.error("Failed to load data: ", err);
                    this.error.set("Failed to load data. Please try again.");
                }
        })
    }

    deleteTransaction(id: string) {
        this.transactionSer.deleteTransaction(id)
            .pipe(takeUntilDestroyed(this.destroyRef))
            .subscribe({
                next: () => {
                    this.transactions.update(list => list.filter(t => t.transactionId !== id)),
                    this.loadData()
                },
                error: err => {
                    console.error('Failed to delete transaction:', err);
                    this.error.set('Failed to delete transaction. Please try again.');
                }
            })
    }

    openEditModalById(id: string): void {
        const transaction = this.transactions()
            .find(t => t.transactionId === id);

        if (!transaction) {
            console.error('Transaction not found');
            this.router.navigate(['/transactions']);
            return;
        }

        this.openEditModal(transaction);
    }

    openAddModal(): void {
        const dialogRef = this.dialog.open<
            AddTransaction,
            Category[] | null,
            NewTransaction
        >(AddTransaction, {
            width: '30rem',
            height: '100%',
            position: {
                top: '0',
                bottom: '0',
                right: '0'
            },
            data: this.categories()
        });

        dialogRef.afterClosed()
            .pipe(
                takeUntilDestroyed(this.destroyRef),
                tap(() => this.router.navigate(['transactions'])),
                filter((result): result is NewTransaction => !!result),
                switchMap(result => this.transactionSer.createTransaction(result))
            )
            .subscribe({
                next: data => {
                    this.loadData();
                    this.transactions.update(t => [...t, data]);
                    this.router.navigate(['/transactions']);
                },
                error: err => {
                    console.error('Create failed', err);
                }
            });
        }



    openEditModal(transaction: Transaction): void {
        const dialogRef = this.dialog.open<
            EditTransaction,
            { transaction: Transaction; categories: Category[] } | null,
            NewTransaction>
        (EditTransaction,
            {
                width: '30rem',
                height: '100%',
                position: {
                    top: '0',
                    bottom: '0',
                    right: '0'
                },
                data: {
                    transaction: transaction,
                    categories: this.categories()
                },
                disableClose: false
            }
        );

        dialogRef.afterClosed()
            .pipe(
                takeUntilDestroyed(this.destroyRef),
                tap(() => this.router.navigate(['/transactions'])),
                filter((result): result is NewTransaction => !!result),
                switchMap(result =>
                    this.transactionSer.updateTransaction(
                        transaction.transactionId,
                        result
                    )             
                )
            )
            .subscribe({
                next: updated => {
                    this.transactions.update(transactions => 
                        transactions.map(t => 
                            t.transactionId === updated.transactionId ? updated : t))
                    this.loadData();
                    this.router.navigate(['/transactions']);
                },
                error: err => {
                    console.error('Update failed', err);
                }
        });
    }

    onEditFromTable(transactionId: string): void {
        this.router.navigate(['/transactions', transactionId, 'edit']);
    }

    onAddTransaction(): void {
        this.router.navigate(['/transactions', 'create']);
    }
}
