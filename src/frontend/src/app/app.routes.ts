import { Routes } from '@angular/router';
import { AuthLayout } from './features/auth/layout/auth-layout/auth-layout';
import { DashboardComponent } from './features/dashboard/component/dashboard/dashboard';

export const routes: Routes = [
    {
        path: '',
        redirectTo: 'auth',
        pathMatch: 'full'
    },
    {
        path: 'auth',
        component: AuthLayout,
        loadChildren: (() => import('./features/auth/routes/auth-route').then(m => m.authRoutes))
    },
    {
        path: '**',
        redirectTo: '/auth/sign-in'
    },
    {
        path: 'dashboard',
        component: DashboardComponent
    }
];
