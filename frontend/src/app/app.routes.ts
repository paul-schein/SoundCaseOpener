import { Routes } from '@angular/router';
import {AdminDashboard} from './admin-dashboard/admin-dashboard';

export const routes: Routes = [
  {path: 'admin-dashboard', component: AdminDashboard, /*canActivate: [IsAdminAuthGuard]*/},
  {path: '', redirectTo: 'admin-dashboard', pathMatch: 'full'},
];
