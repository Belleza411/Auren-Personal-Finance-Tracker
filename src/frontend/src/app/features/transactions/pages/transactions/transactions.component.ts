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
    debounceTime,
    distinctUntilChanged,
    filter,
    startWith,
    switchMap,
    take,
    tap
} from 'rxjs';
import { rxResource, takeUntilDestroyed, toObservable, toSignal } from '@angular/core/rxjs-interop';
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
import { CategoryStateService } from '../../../categories/services/category-state.service'
import { SlidePanelService } from '../../../../core/services/slide-panel.service';
import { AlertService } from 'src/app/core/services/alert.service';

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
    private alert = inject(AlertService);
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
    
    private filters = toSignal(
        toObservable(this.rawFilters).pipe(
            distinctUntilChanged((a, b) =>
                JSON.stringify(a, Object.keys(a).sort()) === JSON.stringify(b, Object.keys(b).sort())
            )
        ),
        { initialValue: this.rawFilters() }
    );

    private reloadTrigger = signal(0);

    transactionResource = rxResource({
        params: () => ({
            filters: this.filters(),
            pageNumber: this.pagination().pageNumber,
            pageSize: this.pagination().pageSize,
            reload: this.reloadTrigger()
        }),
        stream: ({ params }) => 
            this.transactionStateSer.getTransactions(params.filters, params.pageSize, params.pageNumber)
    })

    categoryResource = rxResource({
        stream: () => this.categoryStateService.getCategories({}, 30, 1)
    });
   
    transactions = computed(() => this.transactionResource.value()?.items ?? []);
    categories = computed(() => this.categoryResource.value()?.items ?? []);
    totalCount = computed(() => this.transactionResource.value()?.totalCount ?? 0);
    isLoading = computed(() => this.transactionResource.isLoading());
    pageSize = computed(() => this.pagination().pageSize);
    pageNumber = computed(() => this.pagination().pageNumber);

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
            this.alert.error('Transaction Not Found', `The transaction you are trying to delete could not be found.`);
            return;
        }

        this.transactionStateSer.deleteTransaction(id)
            .pipe(
                take(1),
                takeUntilDestroyed(this.destroyRef),
                tap(() => this.transactionResource.reload())
            )
            .subscribe({
                next: () => this.alert.success('Transaction Deleted', `The transaction has been deleted successfully.`),
                error: () => this.alert.error('Failed to delete transaction', `This transaction could not be deleted. Please try again later.`)
            });
    }

    openAddModal(): void {
        const dialogRef = this.slidePanelService.open<
            AddTransaction,
            Category[],
            NewTransaction>
        (AddTransaction, {
            data: this.categories(),
            height: '100%',
            width: '30rem',
            position: {
                right: '0', 
                top: '0',
                bottom: '0'
            }
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
                    this.transactionResource.reload();
                    this.alert.success('Transaction Added', `The transaction has been added successfully.`);
                },
                error: e => 
                    this.alert.error('Failed to add transaction', e.error?.messages ?? 'Please check the entered details and try again.')          
            });
    }

    openEditModal(transaction: Transaction): void {
        const dialogRef = this.slidePanelService.open<
            EditTransaction,
            { transaction: Transaction; categories: Category[] },
            NewTransaction
        >(EditTransaction, {
            width: '30rem',
            position: {
                right: '0', 
                top: '0',
                bottom: '0'
            },
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
                    this.transactionResource.reload();
                    this.alert.success('Transaction Updated', `The transaction has been updated successfully.`);
                },
                error: () => this.alert.error('Failed to update transaction', `Changes could not be saved. Please check the entered details and try again.`)
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
