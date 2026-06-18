import { ChangeDetectionStrategy, Component } from '@angular/core';
import { CellContext, injectFlexRenderContext } from '@tanstack/angular-table';
import { Transaction } from 'src/app/features/transactions/models/transaction.model';
import { lucideEllipsis } from '@ng-icons/lucide';
import { HlmDropdownMenuImports } from '@spartan-ng/helm/dropdown-menu';
import { provideIcons, NgIcon } from '@ng-icons/core';
import { HlmButtonImports } from '@spartan-ng/helm/button';

@Component({
  selector: 'action-dropdown',
  imports: [HlmDropdownMenuImports, NgIcon, HlmButtonImports],
  providers: [provideIcons({ lucideEllipsis })],
  template: `
      <button hlmBtn variant="ghost" [hlmDropdownMenuTrigger]="open">
        <ng-icon name="lucideEllipsis" />
      </button>

      <ng-template #open>
        <div class="absolute right-20 mt-2 bg-white shadow-lg rounded-md py-1.5 px-1 z-10 gray-border">
          <button hlmBtn variant="outline" (click)="edit()">
            <span class="material-symbols-rounded text-sm">edit</span> Edit
          </button>
          <button hlmBtn variant="outline"(click)="delete()">
            <span class="material-symbols-rounded">delete</span> Delete
          </button>
        </div>
      </ng-template>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ActionDropdown {
  protected readonly context = injectFlexRenderContext<CellContext<Transaction, unknown>>();

  protected edit() {
    this.context.table.options.meta?.onEdit?.(this.context.row.original.id);
  }

  protected delete() {
    this.context.table.options.meta?.onDelete?.(this.context.row.original.id);
  }
}
