import { ChangeDetectionStrategy, Component } from '@angular/core';
import { CellContext, injectFlexRenderContext } from '@tanstack/angular-table';
import { Transaction } from 'src/app/features/transactions/models/transaction.model';
import { lucideEllipsis, lucidePencil, lucideTrash2 } from '@ng-icons/lucide';
import { HlmDropdownMenuImports } from '@spartan-ng/helm/dropdown-menu';
import { provideIcons, NgIcon } from '@ng-icons/core';
import { HlmButtonImports } from '@spartan-ng/helm/button';

@Component({
  selector: 'action-dropdown',
  imports: [HlmDropdownMenuImports, NgIcon, HlmButtonImports],
  providers: [provideIcons({ lucideEllipsis, lucidePencil, lucideTrash2 })],
  template: `
      <button hlmBtn variant="ghost" [hlmDropdownMenuTrigger]="open">
        <ng-icon name="lucideEllipsis" />
      </button>

      <ng-template #open>
        <div class="absolute -right-2 bg-white shadow-lg rounded-md w-32 py-1.5 px-1 z-10 gray-border">
          <button hlmBtn variant="ghost" class="w-full flex justify-start gap-3" (click)="edit()">
            <ng-icon name="lucidePencil" size="1rem" />
            Edit
          </button>
          <button hlmBtn variant="ghost" class="w-full flex justify-start gap-3" (click)="delete()">
            <ng-icon name="lucideTrash2" size="1rem" />
            Delete
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
