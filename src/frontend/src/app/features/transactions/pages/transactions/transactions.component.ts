import { 
    ChangeDetectionStrategy,
    Component,
    computed,
    DestroyRef,
    effect,
    inject,
    input,
    signal   
} from '@angular/core';
import { 
    combineLatest,
    debounceTime,
    distinctUntilChanged,
    filter,
    shareReplay,
    startWith,
    Subject,
    switchMap,
    take,
    tap
} from 'rxjs';
import { takeUntilDestroyed, toObservable, toSignal } from '@angular/core/rxjs-interop';
import { ActivatedRoute, NavigationEnd, Router } from "@angular/router";
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
import { NoopScrollStrategy } from '@angular/cdk/overlay';
import { ToastrService } from '../../../../core/services/toastr.service';
import { SlidePanelService } from '../../services/slide-panel.service';

@Component({
  selector: 'app-transaction',
  imports: [TransactionTable, Filter, PaginationComponent],
  templateUrl: './transactions.component.html',
  styleUrl: './transactions.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class TransactionComponent {
    private transactionStateSer = inject(TransactionStateService);
    private categoryStateService = inject(CategoryStateService);
    private toastr = inject(ToastrService);
    private slidePanelService = inject(SlidePanelService);
    private destroyRef = inject(DestroyRef);
    private router = inject(Router);
    private route = inject(ActivatedRoute);
    private dialog = inject(MatDialog);

    id = input<string | null>(null);
    
    config = signal<FilterKindConfig<TransactionFilter>[]>(TRANSACTION_FILTER_KIND_CONFIG);
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

    selectedTransaction = computed(() => 
        this.transactions().find(t => t.id === this.id())
    );

    hasActiveFilters = computed(() => {
        const f = this.rawFilters();
        return (
            f.searchTerm !== '' ||
            f.transactionType !== null ||
            f.startDate !== null ||
            f.endDate !== null ||
            f.category.length > 0 ||
            f.paymentType !== null
        );
    });

    constructor() {
        this.router.events.pipe(
            filter(e => e instanceof NavigationEnd),
            startWith(null),
            takeUntilDestroyed(this.destroyRef)
        ).subscribe(() => this.tryOpenDialog());

        effect(() => {
            if (!this.isLoading()) {
                this.tryOpenDialog();
            }
        });
    }

    private tryOpenDialog(): void {
        if (this.dialog.openDialogs.length > 0) return;
        if (this.isLoading()) return;

        const childRoute = this.route.firstChild;
        if (!childRoute) return;

        const { openAddModal, openEditModal } = childRoute.snapshot.data;
        const id = childRoute.snapshot.paramMap.get('id');

        if (openAddModal) {
            this.openAddModal();
            return;
        }

        if (openEditModal && id) {
            const transaction = this.transactions().find(t => t.id === id);

            if (transaction) {
                this.openEditModal(transaction);
            } else {
                this.router.navigate(['/transactions']);
            }
        }
    }

    deleteTransaction(id: string) {
        const transaction = this.transactions().find(t => t.id === id);
        if (!transaction) {
            this.toastr.showError('Transaction Not Found', `The transaction you are trying to delete could not be found.`);
            return;
        }

        this.transactionStateSer.deleteTransaction(id)
            .pipe(
                take(1),
                takeUntilDestroyed(this.destroyRef),
                tap(() => this.reload$.next())
            )
            .subscribe({
                next: () => this.toastr.showTransactionToast('Deleted', transaction),
                error: () => this.toastr.showError('Failed to delete transaction', `This transaction could not be deleted. Please try again later.`)
            });
    }

    openAddModal(): void {
        const dialogRef = this.slidePanelService.open<
            AddTransaction,
            Category[],
            NewTransaction>
        (AddTransaction, {
            data: this.categories(),
        });

        dialogRef.afterClosed()
            .pipe(
                take(1),
                takeUntilDestroyed(this.destroyRef),
                tap(() => this.router.navigate(['/transactions'])),
                filter((result): result is NewTransaction => !!result),
                switchMap(result => this.transactionStateSer.createTransaction(result))
            )
            .subscribe({
                next: result => {
                    this.transactionStateSer.clearCache();
                    this.reload$.next();
                    this.toastr.showTransactionToast('Added', result);
                },
                error: () => this.toastr.showError('Failed to add transaction', `Please check the entered details and try again.`)          
            });
    }

    openEditModal(transaction: Transaction): void {
        const dialogRef = this.slidePanelService.open<
            EditTransaction,
            { transaction: Transaction; categories: Category[] },
            NewTransaction
        >(EditTransaction, {
            data: {
                transaction: transaction,
                categories: this.categories()
            }
        });

        dialogRef.afterClosed()
            .pipe(
                take(1),
                takeUntilDestroyed(this.destroyRef),
                tap(() => this.router.navigate(['/transactions'])),
                filter((result): result is NewTransaction => !!result),
                switchMap(result =>
                    this.transactionStateSer.updateTransaction(transaction.id, result)           
                )
            )
            .subscribe({
                next: () => {
                    this.transactionStateSer.clearCache();
                    this.reload$.next();
                    this.toastr.showTransactionToast('Updated', transaction);
                },
                error: () => this.toastr.showError('Failed to update transaction', `Changes could not be saved. Please check the entered details and try again.`)
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
