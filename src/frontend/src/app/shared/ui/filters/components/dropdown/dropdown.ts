import { Component, effect, input, output } from '@angular/core';
import { FilterTypeOption } from '../../models/filter.model';

@Component({
  selector: 'app-dropdown',
  imports: [],
  templateUrl: './dropdown.html',
  styleUrl: './dropdown.css',
})
export class Dropdown<T> {
  readonly options = input.required<FilterTypeOption<T>[]>();
  selected = input.required<T>();

  constructor() {
    effect(() => {
      const selectedOption = this.options().find(option => option.value === this.selected());
      if (!selectedOption) {
        this.selectedChange.emit(this.options()[0].value);
      }
    })
  }

  selectedChange = output<T>();

  onSelectionChange(e: Event) {
    const value = (e.target as HTMLSelectElement).value as T;
    this.selectedChange.emit(value);
  }
}
