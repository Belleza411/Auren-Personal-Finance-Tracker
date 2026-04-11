import { Routes } from "@angular/router";
import { TransactionComponent } from "../pages/transactions/transactions.component";

export const transactionRoutes: Routes = [
    {
        path: '',
        component: TransactionComponent,
        children: [
            {
                path: 'create',
                data: { openAddModal: true },
                children: []
            },
            {
                path: ':id/edit',
                data: { openEditModal: true },
                children: []
            }
        ]
    }
]