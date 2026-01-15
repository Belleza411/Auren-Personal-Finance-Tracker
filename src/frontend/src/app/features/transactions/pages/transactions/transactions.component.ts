import { ChangeDetectionStrategy, Component, computed, DestroyRef, inject, OnInit, resource, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import {  filter, firstValueFrom, Subject, switchMap, tap } from 'rxjs';
import { ActivatedRoute, Router } from "@angular/router";
import { MatDialog } from '@angular/material/dialog';

import { TransactionService } from '../../services/transaction.service';
import { NewTransaction, TimePeriod, Transaction, TransactionFilter } from '../../models/transaction.model';
import { TransactionTable } from "../../components/transaction-table/transaction-table";
import { Category } from '../../../categories/models/categories.model';
import { CategoryService } from '../../../categories/services/category.service';
import { EditTransaction } from '../../components/edit-transaction/edit-transaction';
import { AddTransaction } from '../../components/add-transaction/add-transaction';
import { PaginationComponent } from "../../components/pagination/pagination";

@Component({
  selector: 'app-transaction',
  imports: [TransactionTable, PaginationComponent],
  templateUrl: './transactions.component.html',
  styleUrl: './transactions.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class TransactionComponent implements OnInit {
    private transactionSer = inject(TransactionService);
    private categorySer = inject(CategoryService);
    private destroyRef = inject(DestroyRef);
    private router = inject(Router);
    private route = inject(ActivatedRoute);
    private dialog = inject(MatDialog);

    pageNumber = signal(1);
    pageSize = signal(10);
    totalPage = signal(10);
    selectedRange = signal<TimePeriod>(1);

    readonly pageSizeOptions: number[] = [5, 10, 15, 20, 25];
    timePeriodOptions: string[] = ['All Time', 'This Month', 'Last Month', 'Last 3 Months', 'Last 6 Months', 'This Year'];

    protected readonly dummyCategories = signal<Category[]>([
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

    protected dummyTransactions = signal<Transaction[]>([
        {
            transactionId: '1',
            userId: '1',
            categoryId: '1',
            category: this.dummyCategories()[0],
            transactionType: 1,
            name: 'Freelance Payment',
            amount: 2000.0,
            paymentType: 3,
            transactionDate: 'June 1, 2025',
            createdAt: 'June 1, 2025'
        },
        {
            transactionId: '2',
            userId: '1',
            categoryId: '2',
            category: this.dummyCategories()[1],
            transactionType: 2,
            name: 'Groceries',
            amount: 100.0,
            paymentType: 3,
            transactionDate: 'June 2, 2025',
            createdAt: 'June 2, 2025'
        },
        {
            transactionId: '3',
            userId: '1',
            categoryId: '3',
            category: this.dummyCategories()[2],
            transactionType: 2,
            name: 'Health Insurance',
            amount: 55.0,
            paymentType: 3,
            transactionDate: 'June 10, 2025',
            createdAt: 'June 10, 2025'
        },
        {
            transactionId: '4',
            userId: '1',
            categoryId: '4',
            category: this.dummyCategories()[3],
            transactionType: 2,
            name: 'Gas',
            amount: 50.0,
            paymentType: 3,
            transactionDate: 'June 5, 2025',
            createdAt: 'June 5, 2025'
        },
        {
            transactionId: '5',
            userId: '1',
            categoryId: '1',
            category: this.dummyCategories()[0],
            transactionType: 1,
            name: 'Freelance Payment',
            amount: 1000.0,
            paymentType: 3,
            transactionDate: 'June 11, 2025',
            createdAt: 'June 11, 2025'
        },
        {
            transactionId: '6',
            userId: '1',
            categoryId: '1',
            category: this.dummyCategories()[0],
            transactionType: 1,
            name: 'Freelance Payment',
            amount: 1000.0,
            paymentType: 3,
            transactionDate: 'June 15, 2025',
            createdAt: 'June 15, 2025'
        },
        {
            transactionId: '7',
            userId: '1',
            categoryId: '1',
            category: this.dummyCategories()[0],
            transactionType: 1,
            name: 'Freelance Payment',
            amount: 400.0,
            paymentType: 3,
            transactionDate: 'June 20, 2025',
            createdAt: 'June 20, 2025'
        }
    ]);
    
    currentFilters = signal<TransactionFilter>({
        searchTerm: '',
        transactionType: null,
        minAmount: null,
        maxAmount: null,
        startDate: null,
        endDate: null,
        category: [],
        paymentType: null
    });

    transactions = computed(() => this.transactionResource.value()?.items ?? this.dummyTransactions());
    categories = computed(() => this.categoryResource.value() ?? this.dummyCategories());
    totalCount = computed(() => this.transactionResource.value()?.totalCount ?? 100);
    filters = computed(() => ({...this.currentFilters()}));


    ngOnInit(): void {
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

    transactionResource = resource({
        params: () => ({
            filters: this.filters(),
            pageSize: this.pageSize(),
            pageNumber: this.pageNumber()
        }),
        loader: ({ params }) => {
            console.count('TRANSACTION LOADER');
            const transactions = firstValueFrom(this.transactionSer.getAllTransactions(
                params.filters,
                params.pageSize,
                params.pageNumber
            ));

            return transactions;
        }
    });

    categoryResource = resource({
        loader: () => 
            firstValueFrom(this.categorySer.getAllCategories({}, Number.MAX_SAFE_INTEGER))
    })

    deleteTransaction(id: string) {
        this.transactionSer.deleteTransaction(id)
            .pipe(takeUntilDestroyed(this.destroyRef))
            .subscribe({
                next: () => this.transactionResource.reload(),
                error: err => {
                    console.error('Failed to delete transaction:', err);
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
            panelClass: 'dialog',
            data: this.categories()
        });

        dialogRef.afterClosed()
            .pipe(
                takeUntilDestroyed(this.destroyRef),
                tap(() => this.router.navigate(['/transactions'])),
                filter((result): result is NewTransaction => !!result),
                switchMap(result => this.transactionSer.createTransaction(result))
            )
            .subscribe({
                next: () => this.transactionResource.reload(),
                error: err => console.error('Create failed', err)
                
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
                panelClass: 'dialog',
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
                next: () => this.transactionResource.reload(),
                error: err => console.error('Update failed', err)
        });
    }

    onEditFromTable(transactionId: string): void {
        this.router.navigate(['/transactions', transactionId, 'edit']);
    }

    onAddTransaction(): void {
        this.router.navigate(['/transactions', 'create']);
    }

    onFiltersChange(filters: TransactionFilter) {
        if (JSON.stringify(filters) === JSON.stringify(this.currentFilters())) {
            return;
        }

        this.currentFilters.set(filters);
        this.pageNumber.set(1)
    }

    onPageChange(page: number): void {
        this.pageNumber.set(page);
    }

    onPageSizeChange(size: number): void {
        this.pageSize.set(size);
        this.pageNumber.set(1);
    }

    onRangeChange(e: Event) {
        this.selectedRange.set(
            Number((e.target as HTMLSelectElement).value) + 1
        );
    }
}
