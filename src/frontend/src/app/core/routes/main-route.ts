import { Routes } from "@angular/router";
import { DashboardComponent } from "../../features/dashboard/component/dashboard/dashboard";
import { TransactionComponent } from "../../features/transactions/pages/transactions/transactions.component";
import { EditTransaction } from "../../features/transactions/components/edit-transaction/edit-transaction";

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
                path: ':id/edit',
                component: TransactionComponent,
                data: { openEditModal: true }
            }
        ]
    }
]