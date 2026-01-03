import { Routes } from "@angular/router";
import { TransactionComponent } from "../pages/transactions/transactions.component";

export const transactionRoutes: Routes = [
    {
        path: '',
        component: TransactionComponent,
        pathMatch: 'full'
    },
    {
        path: 'create',
        component: TransactionComponent,
        data: { openAddModal: true }
    },
    {
        path: ':id/edit',
        component: TransactionComponent,
        data: { openEditModal: true }
    }
]