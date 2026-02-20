import { Component, input, output } from '@angular/core';
import { FilterTypeOption } from '../../../features/transactions/models/transaction.model';

@Component({
  selector: 'app-dropdown',
  imports: [],
  templateUrl: './dropdown.html',
  styleUrl: './dropdown.css',
})
export class Dropdown<T> {
  readonly options = input.required<FilterTypeOption<T>[]>();
  selected = input.required<T>();

  selectedChange = output<T>();

  onSelectionChange(e: Event) {
    const value = (e.target as HTMLSelectElement).value as T;
    console.log(value);
    
    this.selectedChange.emit(value);
  }
}
