import { inject, Injectable } from '@angular/core';
import { ToastrService as NgxToastrService } from 'ngx-toastr';
import { Transaction } from '../../features/transactions/models/transaction.model';
import { Category } from '../../features/categories/models/categories.model';

@Injectable({
  providedIn: 'root',
})
export class ToastrService {
  private toastr = inject(NgxToastrService)

  showSuccess(message: string, title?: string) {
    this.toastr.success(message, title);
  }

  showError(message: string, title?: string) {
    this.toastr.error(message, title);
  }

  showTransactionToast(action: 'Added' | 'Updated' | 'Deleted', data: Transaction) {
    let message = '';

    switch (action) {
      case 'Added':
        message = `$${data.amount} for ${data.category} has been recorded.`;
        break;
      case 'Updated':
        message = `Updated to $${data.amount} for ${data.name}.`;
        break;
      case 'Deleted':
        message = `$${data.amount} for ${data.category} has been removed.`;
        break;
    }

    this.toastr.success(message, `Transaction ${action}`);
  }

  showCategoryToast(action: 'Added' | 'Updated' | 'Deleted', data: Category) {
    let message = '';

    switch (action) {
      case 'Added':
        message = `Category ${data.name} has been added.`;
        break;
      case 'Updated':
        message = `Category ${data.name} has been updated.`;
        break;
      case 'Deleted':
        message = `Category ${data.name} has been deleted. Existing transactions were unaffected.`;
        break;
    } 

    this.toastr.success(message, `Category ${action}`);
  }
}
