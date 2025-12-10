import { Route, Routes } from "@angular/router";
import { SignInFormComponent } from "../components/sign-in/sign-in";
import { SignUpFormComponent } from "../components/sign-up/sign-up";

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