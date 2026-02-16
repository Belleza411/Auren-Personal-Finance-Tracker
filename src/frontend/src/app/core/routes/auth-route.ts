import { Routes } from "@angular/router";
import { SignInFormComponent } from "../auth/components/sign-in/sign-in";
import { SignUpFormComponent } from "../auth/components/sign-up/sign-up";

export const authRoutes: Routes = [
    {
        path: '',
        redirectTo: 'sign-in',
        pathMatch: 'full'
    },
    {
        path: 'sign-in',
        component: SignInFormComponent
    },
    {
        path: 'sign-up',
        component: SignUpFormComponent
    }
]