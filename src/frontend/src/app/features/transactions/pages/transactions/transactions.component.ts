import { ChangeDetectionStrategy, Component, computed, DestroyRef, effect, inject, OnInit, resource, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import {  filter, firstValueFrom, switchMap, take, tap } from 'rxjs';
import { ActivatedRoute, Router } from "@angular/router";
import { MatDialog } from '@angular/material/dialog';

import { TransactionService } from '../../services/transaction.service';
import { NewTransaction, Transaction, TransactionFilter } from '../../models/transaction.model';
import { TransactionTable } from "../../components/transaction-table/transaction-table";
import { Category } from '../../../categories/models/categories.model';
import { EditTransaction } from '../../components/edit-transaction/edit-transaction';
import { AddTransaction } from '../../components/add-transaction/add-transaction';
import { TransactionStateService } from '../../services/transaction-state.service';
import { dummyCategories, dummyTransactions } from '../../../../shared/fake-data';
import { Filter } from "../../../../shared/ui/filters/filter/filter";
import { PaginationComponent } from "../../../../shared/ui/pagination/pagination";
import { TRANSACTION_FILTER_KIND_CONFIG } from '../../../../shared/constants/type-options';
import { FilterKindConfig } from '../../../../shared/ui/filters/models/filter.model';

@Component({
  selector: 'app-transaction',
  imports: [TransactionTable, Filter, PaginationComponent],
  templateUrl: './transactions.component.html',
  styleUrl: './transactions.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class TransactionComponent implements OnInit {
    private transactionSer = inject(TransactionService);
    private transactionStateSer = inject(TransactionStateService);
    private destroyRef = inject(DestroyRef);
    private router = inject(Router);
    private route = inject(ActivatedRoute);
    private dialog = inject(MatDialog);

    pageNumber = signal<number>(1);
    pageSize = signal<number>(10);
    config = signal<FilterKindConfig<TransactionFilter>[]>(TRANSACTION_FILTER_KIND_CONFIG);
    editTransactionId = signal<string | null>(null);

    currentFilters = signal<TransactionFilter>({
        searchTerm: '',
        transactionType: null,
        startDate: null,
        endDate: null,
        category: [],
        paymentType: null
    });

    timePeriodOptions: string[] = ['All Time', 'This Month', 'Last Month', 'Last 3 Months', 'Last 6 Months', 'This Year'];
    pageSizeOptions: number[] = [10, 20, 30, 40, 50];

    transactions = computed(() => this.transactionResource.value()?.items ?? []);
    categories = computed(() => this.transactions()
        .map(t => t.category)
        .filter(Boolean) as Category[] ?? []
    );
    totalCount = computed(() => this.transactionResource.value()?.totalCount ?? 0);
    isLoading = computed(() => this.transactionResource.isLoading());

    selectedTransaction = computed(() => {
        const id = this.editTransactionId();
        const transactions = this.transactions();

        if (!id || !transactions) return undefined;

        return transactions.find(t => t.id === id);
    });

    constructor() {
        effect(() => {
            const transaction = this.selectedTransaction();

            if (!transaction || this.dialog.openDialogs.length > 0) return;

            this.openEditModal(transaction);
        })
    }

    ngOnInit(): void {
        this.route.params
            .pipe(takeUntilDestroyed(this.destroyRef))
            .subscribe(params => {
                const transactionId = params['id'];
                const shouldOpenEditModal = this.route.snapshot.data['openEditModal'];
                const shouldOpenAddModal = this.route.snapshot.data['openAddModal'];

                if (transactionId && shouldOpenEditModal) {
                    this.editTransactionId.set(transactionId);
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
            return firstValueFrom(this.transactionStateSer.getTransactions(
                params.filters,
                params.pageSize,
                params.pageNumber
            ));
        },
        defaultValue: {
            items: [],
            pageNumber: 1,
            pageSize: 10,
            totalCount: 0
        }
    });

    deleteTransaction(id: string) {
        this.transactionStateSer.deleteTransaction(id)
            .pipe(take(1), takeUntilDestroyed(this.destroyRef))
            .subscribe({
                next: () => {
                    this.transactionResource.reload();
                },
                error: err => {
                    console.error('Failed to delete transaction:', err);
                }
            });
    }

    openAddModal(): void {
        const dialogRef = this.dialog.open<
            AddTransaction,
            Category[],
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
                take(1),
                takeUntilDestroyed(this.destroyRef),
                tap(() => this.router.navigate(['/transactions'])),
                filter((result): result is NewTransaction => !!result),
                switchMap(result => this.transactionSer.createTransaction(result))
            )
            .subscribe({
                next: () => {
                    this.transactionStateSer.clearCache();
                    this.transactionResource.reload();
                },
                error: err => console.error('Create failed', err)            
            });
    }

    openEditModal(transaction: Transaction): void {
        const dialogRef = this.dialog.open<
            EditTransaction,
            { transaction: Transaction; categories: Category[] },
            NewTransaction
        >(EditTransaction,
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
                take(1),
                takeUntilDestroyed(this.destroyRef),
                tap(() => this.router.navigate(['/transactions'])),
                filter((result): result is NewTransaction => !!result),
                switchMap(result =>
                    this.transactionSer.updateTransaction(
                        transaction.id,
                        result
                    )             
                )
            )
            .subscribe({
                next: () => {
                    this.editTransactionId.set(null);
                    this.transactionStateSer.clearCache();
                    this.transactionResource.reload();
                },
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
        const normalize = (f: TransactionFilter) => JSON.stringify(f, Object.keys(f).sort());
        if (normalize(filters) === normalize(this.currentFilters())) return;
        
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
}
