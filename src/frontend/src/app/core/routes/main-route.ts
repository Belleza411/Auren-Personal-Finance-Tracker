import { Routes } from "@angular/router";
import { DashboardComponent } from "../../features/dashboard/components/dashboard/dashboard";

export const mainRoutes: Routes = [
    {
        path: 'dashboard',
        canActivate: [() => import('../auth/guards/auth-guard').then(m => m.authGuard)],
        component: DashboardComponent
    },
    {
        path: 'transactions',
        canActivate: [() => import('../auth/guards/auth-guard').then(m => m.authGuard)],
        loadChildren: () => import('../../features/transactions/routes/transaction-route')
            .then(m => m.transactionRoutes)
    },
    {
        path: 'categories',
        canActivate: [() => import('../auth/guards/auth-guard').then(m => m.authGuard)],
        loadChildren: () => import('../../features/categories/routes/category-route')
            .then(m => m.categoryRoutes)
    },
    {
        path: 'goals',
        canActivate: [() => import('../auth/guards/auth-guard').then(m => m.authGuard)],
        loadChildren: () => import('../../features/goals/routes/goal-route')
            .then(m => m.goalRoutes)
    }
]