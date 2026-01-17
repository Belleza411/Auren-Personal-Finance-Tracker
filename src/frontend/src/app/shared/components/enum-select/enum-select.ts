import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import { FormField } from '@angular/forms/signals';

@Component({
  selector: 'app-enum-select',
  imports: [FormField],
  templateUrl: './enum-select.html',
  styleUrl: './enum-select.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class EnumSelect<T extends number> {
  label = input.required<string>();
  field = input.required<any>();
  map = input.required<Record<T, string>>();
  placeholder = input<string>('Select');

  get stringKeys(): string[] {
    return Object.keys(this.map());
  }

  getLabel(key: string): string {
    return this.map()[Number(key) as T];
  }
}
