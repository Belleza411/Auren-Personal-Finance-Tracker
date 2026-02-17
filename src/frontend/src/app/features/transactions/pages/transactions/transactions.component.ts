import { ChangeDetectionStrategy, Component, computed, DestroyRef, effect, inject, OnInit, output, resource, signal } from '@angular/core';
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
import { dummyCategories, dummyTransactions } from '../../../../shared/fake-data';

@Component({
  selector: 'app-transaction',
  imports: [TransactionTable],
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

    selectedRange = signal<TimePeriod>(1);

    pageNumber = signal<number>(1);
    pageSize = signal<number>(10);

    timePeriodOptions: string[] = ['All Time', 'This Month', 'Last Month', 'Last 3 Months', 'Last 6 Months', 'This Year'];

    protected readonly dummyCategories = signal<Category[]>(dummyCategories);

    protected dummyTransactions = signal<Transaction[]>(dummyTransactions);
    
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

    options = {
        duration: 1.2,
        separator: ',',
        prefix: '$',
        decimalPlaces: 2
    };

    transactions = computed(() => this.transactionResource.value()?.items ?? []);
    categories = computed(() => this.categoryResource.value()?.items ?? []);
    totalCount = computed(() => this.transactionResource.value()?.totalCount ?? 0);
    isLoading = computed(() => this.transactionResource.isLoading());
    
    hasNoTransactions = computed(() => 
        !this.isLoading() &&
        this.totalCount() === 0 &&
        !this.hasActiveFilters()
    );

    hasNoFilterResults = computed(() =>
        !this.isLoading() &&
        this.totalCount() === 0 &&
        this.hasActiveFilters()
    )

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
            filters: this.currentFilters(),
            pageSize: this.pageSize(),
            pageNumber: this.pageNumber()
        }),
        loader: ({ params }) => {
            return firstValueFrom(this.transactionSer.getAllTransactions(
                params.filters,
                params.pageSize,
                params.pageNumber
            ));

        }
    });

    categoryResource = resource({
        loader: () => 
            firstValueFrom(this.categorySer.getAllCategories({}, 50, 1))
    })

    hasActiveFilters = computed(() => {
        const filter = this.currentFilters();

        const hasSearch = filter.searchTerm.trim().length !== 0;
        const hasType = filter.transactionType !== null;
        const hasCategory = filter.category.length !== 0;
        const hasPayment = filter.paymentType !== null;
        const hasAmount = filter.minAmount !== null|| filter.maxAmount !== null;
        const hasDate = filter.startDate !== null || filter.endDate !== null;

        return hasSearch || hasType || hasCategory || hasPayment || hasAmount || hasDate;
    });

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
        this.pageNumber.set(1);
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
