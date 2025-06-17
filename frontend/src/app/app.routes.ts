import { Routes } from '@angular/router';
import {AdminDashboard} from './admin-dashboard/admin-dashboard';
import {Home} from './home/home';
import {Login} from './login/login';
import {authGuard} from './auth-guard';

export const routes: Routes = [
  {path: 'admin-dashboard', component: AdminDashboard, /*canActivate: [IsAdminAuthGuard]*/},
  {path: '', component: Home, canActivate: [authGuard]},
  {path: 'login', component: Login},
];
