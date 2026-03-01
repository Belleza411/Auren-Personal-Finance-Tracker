import { Component, inject, signal } from '@angular/core';
import { Category, NewCategory } from '../../models/categories.model';
import { MAT_DIALOG_DATA } from '@angular/material/dialog';
import { DialogRef } from '@angular/cdk/dialog';
import { CategoryForm } from "../category-form/category-form";

@Component({
  selector: 'app-add-category',
  imports: [CategoryForm],
  templateUrl: './add-category.html',
  styleUrl: './add-category.css',
})
export class AddCategory {
  protected readonly data: Category[] = inject(MAT_DIALOG_DATA);
  protected dialogRef = inject(DialogRef<NewCategory>);

  protected model = signal<NewCategory>({
    name: '',
    transactionType: "Income"
  })

  onSave(data: NewCategory) {
    this.dialogRef.close(data);
  }
}
