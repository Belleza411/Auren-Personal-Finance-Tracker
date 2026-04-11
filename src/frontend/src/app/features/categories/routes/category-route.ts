import { Routes } from "@angular/router";
import { CategoriesComponent } from "../pages/categories/categories.component";

export const categoryRoutes: Routes = [
    {
        path: '',
        component: CategoriesComponent,
        children: [
            {
                path: 'create',
                data: { openAddModal: true },
                children: []
            },
            {
                path: ':id/edit',
                data: { openEditModal: true },
                children: []
            }
        ]
    }, 
]