import { Routes } from '@angular/router';
import {Home} from './home/home';
import {Login} from './login/login';
import {authGuard} from './auth-guard';

export const routes: Routes = [
  {path: '', component: Home, canActivate: [authGuard]},
  {path: 'login', component: Login},
];
