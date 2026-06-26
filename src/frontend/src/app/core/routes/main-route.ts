import { Routes } from "@angular/router";
import { DashboardComponent } from "../../features/dashboard/components/dashboard/dashboard.component";
import { authGuard } from "../auth/guards/auth.guard";
import { Profile } from "src/app/features/profile/pages/profile/profile";

export const mainRoutes: Routes = [
    {
        path: 'dashboard',
        canActivate: [authGuard],
        component: DashboardComponent
    },
    {
        path: 'transactions',
        canActivate: [authGuard],
        loadChildren: () => import('../../features/transactions/routes/transaction-route')
            .then(m => m.transactionRoutes)
    },
    {
        path: 'categories',
        canActivate: [authGuard],
        loadChildren: () => import('../../features/categories/routes/category-route')
            .then(m => m.categoryRoutes)
    },
    {
        path: 'profile',
        canActivate: [authGuard],
        loadComponent: () => 
            import('./../../features/profile/pages/profile/profile')
                .then(c => c.Profile),
        children: [
            {
                path: 'delete-account',
                loadComponent: () => 
                    import('./../../features/profile/components/delete-account/delete-account')
                        .then(c => c.DeleteAccount)
            }
        ]
    }
]