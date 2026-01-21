import { Routes } from "@angular/router";
import { CategoriesComponent } from "../pages/categories/categories.component";

export const categoryRoutes: Routes = [
    {
        path: '',
        component: CategoriesComponent,
        pathMatch: 'full'
    }, 
    {
        path: 'create',
        component: CategoriesComponent,
        data: { openAddModal: true }
    },
    {
        path: ':id/create',
        component: CategoriesComponent,
        data: { openEditModal: true }
    }
]