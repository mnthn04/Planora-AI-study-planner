import { Routes } from '@angular/router';
import { LoginComponent } from './pages/login/login.component';
import { RegisterComponent } from './pages/register/register.component';
import { DashboardComponent } from './pages/dashboard/dashboard.component';
import { AddSubjectComponent } from './pages/add-subject/add-subject.component';
import { PlanViewComponent } from './pages/plan-view/plan-view.component';
import { authGuard } from './auth.guard';

export const routes: Routes = [
    { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
    { path: 'login', component: LoginComponent },
    { path: 'register', component: RegisterComponent },
    { path: 'dashboard', component: DashboardComponent, canActivate: [authGuard] },
    { path: 'add-subject', component: AddSubjectComponent, canActivate: [authGuard] },
    { path: 'edit-subject/:id', component: AddSubjectComponent, canActivate: [authGuard] },
    { path: 'plan/:id', component: PlanViewComponent, canActivate: [authGuard] },
];
