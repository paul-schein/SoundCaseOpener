import { CanActivateFn , Router} from '@angular/router';
import {LoginService} from '../core/login.service';
import { inject } from '@angular/core';

export const authGuard: CanActivateFn = (route, state) => {
  const service: LoginService = inject(LoginService);
  const router: Router = inject(Router);

  let isLoggedin = service.currentUser !== null;

  if(!isLoggedin) {
    router.navigate(['/login']);
    return isLoggedin;
  }

  return isLoggedin;
};
