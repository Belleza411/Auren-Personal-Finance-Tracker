import { Routes } from '@angular/router';
import { AuthLayout } from './features/auth/layout/auth-layout/auth-layout';

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
        redirectTo: 'sign-in'
    }
];
