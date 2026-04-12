import { Component, inject, signal } from '@angular/core';
import { NewCategory } from '../../models/categories.model';
import { CategoryForm } from "../category-form/category-form";
import { MatDialogRef } from '@angular/material/dialog';

@Component({
  selector: 'app-add-category',
  imports: [CategoryForm],
  templateUrl: './add-category.html',
  styleUrl: './add-category.css',
})
export class AddCategory {
  protected dialogRef = inject(MatDialogRef<NewCategory>);

  protected model = signal<NewCategory>({
    name: '',
    transactionType: "Income"
  })

  onSave(data: NewCategory) {
    this.dialogRef.close(data);
  }
}
