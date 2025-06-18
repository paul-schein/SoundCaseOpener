import { CanActivateFn , Router} from '@angular/router';
import {LoginService, User} from '../core/services/login-service';
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

export const isAdminAuthGuard: CanActivateFn = async (route, state) => {
  const service: LoginService = inject(LoginService);
  const router: Router = inject(Router);

  const currentUser: User | null = service.currentUser();

  if (currentUser === null ||  currentUser.role !== 'Admin') {
    await router.navigate(['/home']);
    return false;
  }
  return true;
}
