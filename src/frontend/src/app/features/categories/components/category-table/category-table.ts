import { Component, input, output, signal } from '@angular/core';
import { Category } from '../../models/categories.model';
import { UpperCasePipe } from '@angular/common';
import { TransactionTypeColorPipe } from "../../../transactions/pipes/transaction-type-color.pipe";

@Component({
  selector: 'app-category-table',
  imports: [UpperCasePipe, TransactionTypeColorPipe],
  templateUrl: './category-table.html',
  styleUrl: './category-table.css',
})
export class CategoryTable {
  categories = input.required<Category[]>();
  isLoading = input.required<boolean>();

  delete = output<string>();
  edit = output<string>();

  openModalId = signal<string | null>(null);

  toggleModalId(id: string) {
    this.openModalId.update(current =>
      current === id ? null : id
    );
  }

  onDelete(id: string) {
    this.delete.emit(id);
  }

  onEdit(id: string) {
    this.edit.emit(id);
  }
}
