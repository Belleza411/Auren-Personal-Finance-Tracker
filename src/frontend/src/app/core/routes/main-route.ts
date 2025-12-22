import { Routes } from "@angular/router";
import { DashboardComponent } from "../../features/dashboard/component/dashboard/dashboard";
import { TransactionComponent } from "../../features/transactions/pages/transactions/transactions.component";

export const mainRoutes: Routes = [
    {
        path: 'dashboard',
        component: DashboardComponent
    },
    {
        path: 'transactions',
        component: TransactionComponent,
        children: [
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
    }
]