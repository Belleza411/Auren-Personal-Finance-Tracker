import { ChangeDetectionStrategy, Component, effect, input, output } from '@angular/core';
import { FilterTypeOption } from '../../models/filter.model';
import { 
  HlmSelectContent,
  HlmSelectGroup,
  HlmSelectItem,
  HlmSelectPortal,
  HlmSelect,
  HlmSelectTrigger,
  HlmSelectValue 
} from '../../../../../libs/ui/select/src';

@Component({
  selector: 'app-dropdown',
  imports: [
    HlmSelectContent,
    HlmSelectGroup,
    HlmSelectItem,
    HlmSelectPortal,
    HlmSelect,
    HlmSelectTrigger,
    HlmSelectValue
],
  template: `
    <hlm-select 
        [value]="selected()"
        [itemToString]="itemToString"
        (valueChange)="onSelectionChange($event)"
    >
        <hlm-select-trigger class="w-56">
            <hlm-select-value placeholder="Select an option" />
        </hlm-select-trigger>

        <hlm-select-content *hlmSelectPortal>
            <hlm-select-group>
                @for (option of options(); track option.value) {
                    <hlm-select-item [value]="option.value">
                        {{ option.label }}
                    </hlm-select-item>
                }
            </hlm-select-group>
        </hlm-select-content>
    </hlm-select>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class Dropdown<T> {
  readonly options = input.required<FilterTypeOption<T>[]>();
  selected = input.required<T>();

  constructor() {
    effect(() => {
        const opts = this.options();
        if (!opts.length) return;
        const found = opts.find(o => o.value === this.selected());
        if (!found) this.selectedChange.emit(opts[0].value);
    });
  }

  readonly itemToString = (value: T): string => 
    value != null ? String(value) : '';

  selectedChange = output<T>();

  onSelectionChange(value: T | null | undefined) {
    if (value != null) {
      this.selectedChange.emit(value);
    }
  }
}
