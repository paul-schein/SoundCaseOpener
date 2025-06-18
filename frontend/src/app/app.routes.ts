import { Routes } from '@angular/router';
import {Home} from './home/home';
import {Login} from './login/login';
import {authGuard} from './auth-guard';
import {LobbyList} from './lobby-list/lobby-list';
import {LobbyDetail} from './lobby-list/lobby-detail/lobby-detail';

export const routes: Routes = [
  {path: 'home', component: Home, canActivate: [authGuard]},
  {path: 'login', component: Login},
  {path: 'lobby-list', component: LobbyList, canActivate: [authGuard]},
  {path: 'lobby/:lobbyId', component: LobbyDetail, canActivate: [authGuard]},
  {path: '**', redirectTo: 'login', pathMatch: 'full'},
];
