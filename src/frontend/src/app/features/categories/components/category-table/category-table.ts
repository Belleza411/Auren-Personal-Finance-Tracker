import { ChangeDetectionStrategy, Component, inject, input, output, signal } from '@angular/core';
import { Category } from '../../models/categories.model';
import { UpperCasePipe } from '@angular/common';
import { TransactionTypeColorPipe } from "../../../transactions/pipes/transaction-type-color.pipe";
import { NavigationEnd, Router } from '@angular/router';
import { 
  type ColumnFiltersState,
  type RowSelectionState,
  type SortingState,
  type VisibilityState,
  type ColumnDef,
  getCoreRowModel,
  getFilteredRowModel,
  getPaginationRowModel,
  getSortedRowModel
} from '@tanstack/table-core';
import { createAngularTable, flexRenderComponent, FlexRenderDirective } from '@tanstack/angular-table';
import { TableHeadSelection } from 'src/app/shared/components/table-head-selection/table-head-selection';
import { TableRowSelection } from 'src/app/shared/components/table-row-selection/table-row-selection';
import { TableHeadSortButton } from 'src/app/shared/components/table-head-sort-button/table-head-sort-button';
import { TransactionTypeBadge } from 'src/app/shared/components/transaction-type-badge/transaction-type-badge';
import { TransactionType } from 'src/app/features/transactions/models/transaction.model';
import { ActionDropdown } from 'src/app/shared/components/action-dropdown/action-dropdown';
import { HlmSkeleton } from "@spartan-ng/helm/skeleton";
import { 
	HlmTable,
	HlmTableContainer,
	HlmTBody,
	HlmTd,
	HlmTHead,
	HlmTr,
} from "../../../../libs/ui/table/src/lib/hlm-table"
@Component({
  selector: 'app-category-table',
  imports: [
    HlmSkeleton,
    FlexRenderDirective,
    HlmTable,
    HlmTableContainer,
    HlmTBody,
    HlmTd,
    HlmTHead,
    HlmTr
  ],
  templateUrl: './category-table.html',
  styleUrl: './category-table.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class CategoryTable {
  private router = inject(Router);
  categories = input.required<Category[]>();
  isLoading = input.required<boolean>();

  delete = output<string>();
  edit = output<string>();

  private readonly _columnFilters = signal<ColumnFiltersState>([]);
  private readonly _sorting = signal<SortingState>([]);
  private readonly _rowSelection = signal<RowSelectionState>({});
  private readonly _columnVisibility = signal<VisibilityState>({});

  protected readonly _columns: ColumnDef<Category>[] = [
    {
      id: 'select',
      header: () => flexRenderComponent(TableHeadSelection),
      cell: () => flexRenderComponent(TableRowSelection),
      enableSorting: false,
      enableHiding: false,
    },
    {
      accessorKey: 'name',
      id: 'name',
      header: () => flexRenderComponent(TableHeadSortButton, { inputs: { header: 'Title' } }),
      cell: info => `<p>${info.getValue<string>()}</p>`
    },
    {
      accessorKey: 'transactionType',
      id: 'type',
      header: () => flexRenderComponent(TableHeadSortButton, { inputs: { header: 'Type' } }),
      cell: (info) => flexRenderComponent(TransactionTypeBadge, { inputs: { type: info.getValue<TransactionType>() } }),
    },
    {
      id: 'actions',
      enableHiding: false,
      cell: () => flexRenderComponent(ActionDropdown, { inputs: {} }),
    },
  ]

  protected readonly _table = createAngularTable<Category>(() => ({
    data: this.categories(),
    columns: this._columns,
    getCoreRowModel: getCoreRowModel(),
    getFilteredRowModel: getFilteredRowModel(),
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

  onDelete(id: string) {
    this.delete.emit(id);
  }

  onEdit(id: string) {
    this.edit.emit(id);
  }
}
