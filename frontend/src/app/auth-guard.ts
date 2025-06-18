import { CanActivateFn , Router} from '@angular/router';
import {LoginService} from '../core/login.service';
import { inject } from '@angular/core';

export const authGuard: CanActivateFn = async (route, state) => {
  const service: LoginService = inject(LoginService);
  const router: Router = inject(Router);

  let isLoggedIn: boolean = service.currentUser() !== null || await service.tryLoadCurrentUserAsync();

  if(!isLoggedIn) {
    await router.navigate(['/login']);
    return isLoggedIn;
  }

  return isLoggedIn;
};
