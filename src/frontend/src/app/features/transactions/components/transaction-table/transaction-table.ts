import { ChangeDetectionStrategy, Component, computed, inject, input, OnInit, output, signal } from '@angular/core';

import { Transaction, TransactionType } from '../../models/transaction.model';
import { COMPACT_TRANSACTION_COLUMNS, FULL_TRANSACTION_COLUMNS } from '../../models/transaction-column.model';
import { Router } from '@angular/router';
import { 
	HlmTable,
	HlmTableContainer,
	HlmTBody,
	HlmTd,
	HlmTHead,
	HlmTr,
} from "../../../../libs/ui/table/src/lib/hlm-table"
import { HlmSkeleton } from '../../../../libs/ui/skeleton/src/lib/hlm-skeleton';

import {
	type ColumnDef,
	type ColumnFiltersState,
	createAngularTable,
	flexRenderComponent,
	FlexRenderDirective,
	getCoreRowModel,
	getFilteredRowModel,
	getPaginationRowModel,
	getSortedRowModel,
	type RowSelectionState,
	type SortingState,
  type RowData,
  type VisibilityState,
} from '@tanstack/angular-table';

import { ActionDropdown } from 'src/app/shared/components/action-dropdown/action-dropdown';
import { TableRowSelection } from 'src/app/shared/components/table-row-selection/table-row-selection';
import { TableHeadSortButton } from 'src/app/shared/components/table-head-sort-button/table-head-sort-button';
import { TableHeadSelection } from 'src/app/shared/components/table-head-selection/table-head-selection';

import { TransactionTypeBadge } from 'src/app/shared/components/transaction-type-badge/transaction-type-badge';

declare module '@tanstack/angular-table' {
  interface TableMeta<TData extends RowData> {
    onEdit?: (id: string) => void;
    onDelete?: (id: string) => void;
  }
}

@Component({
  selector: 'app-transaction-table',
  imports: [
    HlmTable,
    HlmTableContainer,
    HlmTBody,
    HlmTd,
    HlmTHead,
    HlmTr,
    HlmSkeleton,
    FlexRenderDirective,
  ],
  templateUrl: './transaction-table.html',
  styleUrl: './transaction-table.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: {
    '[class.compact]': "variant() === 'compact'"
  }
})
export class TransactionTable {
  transactions = input.required<Transaction[]>();
  isLoading = input<boolean>();
  variant = input<'full' | 'compact'>('full');

  columns = computed(() =>
    this.variant() === 'compact'
      ? COMPACT_TRANSACTION_COLUMNS
      : FULL_TRANSACTION_COLUMNS
  );

  private readonly _columnFilters = signal<ColumnFiltersState>([]);
	private readonly _sorting = signal<SortingState>([]);
	private readonly _rowSelection = signal<RowSelectionState>({});
  private readonly _columnVisibility = signal<VisibilityState>({});

  protected readonly _columns: ColumnDef<Transaction>[] = [
    {
      id: 'select',
			header: () => flexRenderComponent(TableHeadSelection),
			cell: () => flexRenderComponent(TableRowSelection),
			enableSorting: false,
			enableHiding: false,
    },
    {
      accessorKey: 'transactionDate',
      id: 'transactionDate',
      header: () => flexRenderComponent(TableHeadSortButton, { inputs: { header: 'Transaction Date' } }),
      cell: info => `<p>${info.getValue<string>()}</p>`
    },
    {
      accessorKey: 'name',
      id: 'name',
      header: () => flexRenderComponent(TableHeadSortButton, { inputs: { header: 'Title' } }),
      cell: info => `<p>${info.getValue<string>()}</p>`
    },
    {
      accessorKey: 'category.name',
      id: 'category',
      header: () => flexRenderComponent(TableHeadSortButton, { inputs: { header: 'Category' } }),
      cell: info => `<p>${info.getValue<string>()}</p>`
    },
    {
      accessorKey: 'transactionType',
      id: 'type',
      header: () => flexRenderComponent(TableHeadSortButton, { inputs: { header: 'Type' } }),
      cell: (info) => flexRenderComponent(TransactionTypeBadge, { inputs: { type: info.getValue<TransactionType>() } }),
    },
    {
			accessorKey: 'amount',
			id: 'amount',
			header: '<p>Amount</p>',
			enableSorting: false,
			cell: info => {
				const amount = parseFloat(info.getValue<string>());
				const formatted = new Intl.NumberFormat('en-US', {
					style: 'currency',
					currency: 'USD',
				}).format(amount);

				return `<p>${formatted}</p>`;
			},
		},
    {
      accessorKey: 'createdAt',
      id: 'createdAt',
      header: () => flexRenderComponent(TableHeadSortButton, { inputs: { header: 'Date Created' } }),
      cell: info => `<p>${info.getValue<string>()}</p>`
    },
    {
      accessorKey: 'paymentType',
      id: 'paymentType',
      header: () => flexRenderComponent(TableHeadSortButton, { inputs: { header: 'Payment Method' } }),
      cell: info => `<p>${info.getValue<string>()}</p>`
    },
    {
			id: 'actions',
			enableHiding: false,
			cell: () => flexRenderComponent(ActionDropdown, { inputs: {} }),
		},
  ];

  protected readonly _table = createAngularTable<Transaction>(() => ({
    data: this.transactions(),
    columns: this._columns,
    getCoreRowModel: getCoreRowModel(),
    getFilteredRowModel: getFilteredRowModel(),
    getPaginationRowModel: getPaginationRowModel(),
    getSortedRowModel: getSortedRowModel(),
    getRowId: (row) => row.id,
    onSortingChange: (updater) => {
			updater instanceof Function ? this._sorting.update(updater) : this._sorting.set(updater);
		},
		onColumnFiltersChange: (updater) => {
			updater instanceof Function ? this._columnFilters.update(updater) : this._columnFilters.set(updater);
		},
		onRowSelectionChange: (updater) => {
			updater instanceof Function ? this._rowSelection.update(updater) : this._rowSelection.set(updater);
		},
    onColumnVisibilityChange: (updater) => {
        updater instanceof Function 
          ? this._columnVisibility.update(updater) 
          : this._columnVisibility.set(updater);
    },
		state: {
			sorting: this._sorting(),
			columnFilters: this._columnFilters(),
			rowSelection: this._rowSelection(),
			columnVisibility: this._columnVisibility(),
		},
    meta: {
      onEdit: (id: string) => this.onEdit(id),
      onDelete: (id: string) => this.onDelete(id),
    },
  }))
  
  delete = output<string>();
  edit = output<string>(); 

  onDelete(id: string) {
    this.delete.emit(id);
  }

  onEdit(id: string) {
    this.edit.emit(id);
  }
}  
