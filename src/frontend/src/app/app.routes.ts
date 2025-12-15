import { Routes } from '@angular/router';
import { AuthLayout } from './core/layout/auth-layout/auth-layout';
import { DashboardComponent } from './features/dashboard/component/dashboard/dashboard';
import { MainLayoutComponent } from './core/layout/main-layout/main-layout';

export const routes: Routes = [
    {
        path: '',
        redirectTo: 'auth',
        pathMatch: 'full'
    },
    {
        path: 'auth',
        component: AuthLayout,
        loadChildren: (() => import('./core/routes/auth-route').then(m => m.authRoutes))
    },
    {
        path: '',
        component: MainLayoutComponent,
        loadChildren: (() => import('./core/routes/main-route').then(m => m.mainRoutes))
    }
];
