import { ChangeDetectionStrategy, Component, computed, input, output, signal } from '@angular/core';
import { DropdownKind, DropdownLabelOption } from '../../models/filter.model';

@Component({
  selector: 'app-dropdown-with-modal',
  imports: [],
  templateUrl: './dropdown-with-modal.html',
  styleUrl: './dropdown-with-modal.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class DropdownWithModal<T> {
  kind = input.required<DropdownKind>();
  start = input.required<T | null>();
  end = input.required<T | null>();
  labelOption = input.required<DropdownLabelOption>();

  isOpen = signal(false);
  onToggle() { 
    this.isOpen.update(v => !v); 
  }

  startChange = output<T | null>();
  endChange = output<T | null>();
  clear = output<void>();

  hasValue = computed(() => this.start() !== null && this.end() !== null);

  onStartChange(e: Event) {
    const value = (e.target as HTMLInputElement).value as T; 
    this.startChange.emit(value);
  }

  onEndChange(e: Event) {
    const value = (e.target as HTMLInputElement).value as T;
    this.endChange.emit(value);
  }
}
