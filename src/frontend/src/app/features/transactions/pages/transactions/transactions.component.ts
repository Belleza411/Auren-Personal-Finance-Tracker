import { ChangeDetectionStrategy, Component, computed, DestroyRef, effect, inject, OnInit, signal } from '@angular/core';
import { takeUntilDestroyed, toObservable, toSignal } from '@angular/core/rxjs-interop';
import { combineLatest, debounceTime, distinctUntilChanged, filter, shareReplay, startWith, Subject, switchMap, take, tap } from 'rxjs';
import { ActivatedRoute, Router } from "@angular/router";
import { MatDialog } from '@angular/material/dialog';

import { NewTransaction, Transaction, TransactionFilter } from '../../models/transaction.model';
import { TransactionTable } from "../../components/transaction-table/transaction-table";
import { Category } from '../../../categories/models/categories.model';
import { EditTransaction } from '../../components/edit-transaction/edit-transaction';
import { AddTransaction } from '../../components/add-transaction/add-transaction';
import { TransactionStateService } from '../../services/transaction-state.service';
import { Filter } from "../../../../shared/ui/filters/filter/filter";
import { PaginationComponent } from "../../../../shared/ui/pagination/pagination";
import { TRANSACTION_FILTER_KIND_CONFIG } from '../../../../shared/constants/type-options';
import { FilterKindConfig } from '../../../../shared/ui/filters/models/filter.model';
import { CategoryStateService } from '../../../categories/services/category-state.service';

@Component({
  selector: 'app-transaction',
  imports: [TransactionTable, Filter, PaginationComponent],
  templateUrl: './transactions.component.html',
  styleUrl: './transactions.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class TransactionComponent implements OnInit {
    private transactionStateSer = inject(TransactionStateService);
    private categoryStateService = inject(CategoryStateService);
    private destroyRef = inject(DestroyRef);
    private router = inject(Router);
    private route = inject(ActivatedRoute);
    private dialog = inject(MatDialog);

    config = signal<FilterKindConfig<TransactionFilter>[]>(TRANSACTION_FILTER_KIND_CONFIG);
    editTransactionId = signal<string | null>(null);
    private pagination = signal({ pageNumber: 1, pageSize: 10});

    rawFilters = signal<TransactionFilter>({
        searchTerm: '',
        transactionType: null,
        startDate: null,
        endDate: null,
        category: [],
        paymentType: null
    });

    timePeriodOptions: string[] = ['All Time', 'This Month', 'Last Month', 'Last 3 Months', 'Last 6 Months', 'This Year'];
    pageSizeOptions: number[] = [10, 20, 30, 40, 50];
    
    private debouncedFilter$ = toObservable(this.rawFilters).pipe(
        debounceTime(300),
        distinctUntilChanged((a, b) =>
            JSON.stringify(a, Object.keys(a).sort()) === JSON.stringify(b, Object.keys(b).sort())
        )
    );
    private pageNumber$ = toObservable(computed(() => this.pagination().pageNumber));
    private pageSize$ = toObservable(computed(() => this.pagination().pageSize));
    private reload$ = new Subject<void>();

    private transactionData$ = combineLatest([
        this.debouncedFilter$,
        this.pageNumber$,
        this.pageSize$,
        this.reload$.pipe(startWith(null))
    ]).pipe(
        debounceTime(300),
        switchMap(([filters, pageNumber, pageSize]) =>
            this.transactionStateSer.getTransactions(filters, pageSize, pageNumber).pipe(
                startWith(null)
            )
        ),
        shareReplay(1)
    );

    private categoryData$ = this.categoryStateService.getCategories({}, 30, 1)
        .pipe(
            shareReplay(1)
        );
        
    transactionData = toSignal(this.transactionData$, { initialValue: null });
    categoryData = toSignal(this.categoryData$, { initialValue: null })
    transactions = computed(() => this.transactionData()?.items ?? []);
    categories = computed(() => this.categoryData()?.items ?? [])
    totalCount = computed(() => this.transactionData()?.totalCount ?? 0);
    isLoading = computed(() => this.transactionData() === null);
    
    pageSize = toSignal(this.pageSize$, { initialValue: 10 })
    pageNumber = toSignal(this.pageNumber$, { initialValue: 1 })

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

    deleteTransaction(id: string) {
        this.transactionStateSer.deleteTransaction(id)
            .pipe(
                take(1),
                takeUntilDestroyed(this.destroyRef),
                tap(() => this.reload$.next())
            )
            .subscribe();
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
                filter((result): result is NewTransaction => !!result),
                switchMap(result => this.transactionStateSer.createTransaction(result))
            )
            .subscribe({
                next: () => {
                    this.transactionStateSer.clearCache();
                    this.reload$.next();
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
                filter((result): result is NewTransaction => !!result),
                tap(() => this.router.navigate(['/transactions'])),
                switchMap(result =>
                    this.transactionStateSer.updateTransaction(transaction.id, result)           
                )
            )
            .subscribe({
                next: () => {
                    this.editTransactionId.set(null);
                    this.transactionStateSer.clearCache();
                    this.reload$.next();
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
        this.rawFilters.set(filters);
        this.onPageChange(1);
    }

    onPageSizeChange(size: number) {
        this.pagination.set({ pageNumber: 1, pageSize: size }); 
    }

    onPageChange(page: number) {
        this.pagination.update(p => ({ ...p, pageNumber: page }));
    }
}
