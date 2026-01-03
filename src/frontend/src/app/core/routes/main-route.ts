import { Routes } from "@angular/router";
import { DashboardComponent } from "../../features/dashboard/component/dashboard/dashboard";

export const mainRoutes: Routes = [
    {
        path: 'dashboard',
        component: DashboardComponent
    },
    {
        path: 'transactions',
        loadChildren: () => import('../../features/transactions/routes/transaction-route')
            .then(m => m.transactionRoutes)
    }
]