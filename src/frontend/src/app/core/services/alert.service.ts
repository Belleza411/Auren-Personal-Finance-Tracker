import { Service } from '@angular/core';
import { BehaviorSubject } from 'rxjs';
import { Alert, AlertType, CrudAction } from '../models/alert.model';
import { Category } from '../../features/categories/models/categories.model';
import { Transaction } from '../../features/transactions/models/transaction.model';

@Service()
export class AlertService {
    private readonly duration = 5000;
    private readonly exitDuration = 150;
    private alertsSubject = new BehaviorSubject<Alert[]>([]);

    alerts$ = this.alertsSubject.asObservable();

    show(type: AlertType, message: string, title?: string) {
        const alert: Alert = {
            id: crypto.randomUUID(),
            type,
            title,
            message
        };

        this.alertsSubject.next([...this.alertsSubject.value, alert]);

        setTimeout(() => {
            this.startRemove(alert.id);
        }, this.duration);
    }

    success(message: string, title?: string) {
        this.show('success', message, title);
    }

    error(message: string, title?: string) {
        this.show('error', message, title);
    }

    private startRemove(id: string) {
        const alerts = this.alertsSubject.value;

        this.alertsSubject.next(
            alerts.map(a =>
                a.id === id
                    ? { ...a, leaving: true }
                    : a
            )
        );

        setTimeout(() => {
            this.alertsSubject.next(
            this.alertsSubject.value.filter(a => a.id !== id)
            );
        }, this.exitDuration);
    }


    transaction(
        action: CrudAction,
        transaction: Transaction
    ) {
        const messages: Record<CrudAction, string> = {
            Added: `$${transaction.amount} for ${transaction.name} has been recorded.`,
            Updated: `Updated to $${transaction.amount} for ${transaction.name}.`,
            Deleted: `$${transaction.amount} for ${transaction.name} has been removed.`
        };

        this.success(
            messages[action],
            `Transaction ${action}`
        );
    }

    category(
        action: CrudAction,
        category: Category
    ) {
        const messages: Record<CrudAction, string> = {
            Added: `Category ${category.name} has been added.`,
            Updated: `Category ${category.name} has been updated.`,
            Deleted: `Category ${category.name} has been deleted. Existing transactions were unaffected.`
        };

        this.success(
            messages[action],
            `Category ${action}`
        );
    }
}
