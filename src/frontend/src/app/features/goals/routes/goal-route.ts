import { Routes } from "@angular/router";
import { GoalsComponent } from "../pages/goals/goals.component";

export const goalRoutes: Routes = [
    {
        path: '',
        component: GoalsComponent,
        pathMatch: 'full'
    },
    {
        path: 'create',
        component: GoalsComponent,
        data: { openAddModal: true }
    },
    {
        path: ':id/edit',
        component: GoalsComponent,
        data: { openEditModal: true }
    },
    {
        path: ':id/add-money',
        component: GoalsComponent,
        data: { openAddMoneyModal: true }
    }
]