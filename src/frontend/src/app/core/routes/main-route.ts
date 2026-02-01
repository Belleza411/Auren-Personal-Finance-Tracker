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
    },
    {
        path: 'categories',
        loadChildren: () => import('../../features/categories/routes/category-route')
            .then(m => m.categoryRoutes)
    },
    {
        path: 'goals',
        loadChildren: () => import('../../features/goals/routes/goal-route')
            .then(m => m.goalRoutes)
    }
]