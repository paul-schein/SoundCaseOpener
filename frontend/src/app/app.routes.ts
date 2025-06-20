import { Routes } from '@angular/router';
import {AdminDashboard} from './admin-dashboard/admin-dashboard';
import {Home} from './home/home';
import {Login} from './login/login';
import {authGuard, isAdminAuthGuard} from './auth-guard';
import {LobbyList} from './lobby-list/lobby-list';
import {LobbyDetail} from './lobby-list/lobby-detail/lobby-detail';

const adminDashboardImport = () =>
  import('./admin-dashboard/admin-dashboard').then(m => m.AdminDashboard);

export const routes: Routes = [
  {path: 'home', component: Home, canActivate: [authGuard]},
  {path: 'admin-dashboard', loadComponent: adminDashboardImport, canActivate: [authGuard, isAdminAuthGuard] },
  {path: 'login', component: Login},
  {path: 'lobby-list', component: LobbyList, canActivate: [authGuard]},
  {path: 'lobby/:lobbyId', component: LobbyDetail, canActivate: [authGuard]},
  {path: '**', redirectTo: 'login', pathMatch: 'full'},
];
