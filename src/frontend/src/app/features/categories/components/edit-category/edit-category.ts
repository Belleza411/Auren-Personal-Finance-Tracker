import { DialogRef } from '@angular/cdk/dialog';
import { Component, inject, signal } from '@angular/core';
import { MAT_DIALOG_DATA } from '@angular/material/dialog';
import { Category, NewCategory } from '../../models/categories.model';
import { CategoryForm } from "../category-form/category-form";

@Component({
  selector: 'app-edit-category',
  imports: [CategoryForm],
  templateUrl: './edit-category.html',
  styleUrl: './edit-category.css',
})
export class EditCategory {
  protected readonly data: Category = inject(MAT_DIALOG_DATA);
  protected dialogRef = inject(DialogRef<NewCategory>);

  ngOnInit(): void {
    this.model.set({
      name: this.data.name,
      transactionType: this.data.transactionType
    })
  }

  protected model = signal<NewCategory>({
    name: '',
    transactionType: 1
  })

  onSave(data: NewCategory) {
    this.dialogRef.close(data);
  }
}
