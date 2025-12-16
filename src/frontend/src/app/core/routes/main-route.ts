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
        component: TransactionComponent
    }
]