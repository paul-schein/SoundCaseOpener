import {Component, computed, inject, Signal} from '@angular/core';
import {LoginService} from '../../core/login.service';
import {Router, RouterLink} from '@angular/router';
import {MatButton} from '@angular/material/button';


@Component({
  selector: 'app-navbar',
  imports: [
    MatButton,
    RouterLink,
  ],
  templateUrl: './navbar.html',
  styleUrl: './navbar.scss'
})
export class Navbar {
  protected readonly isAdmin: Signal<boolean> = computed(() => {
    return this.service.currentUser()?.role === 'Admin';
  });
  private readonly service: LoginService = inject(LoginService);
  private readonly router: Router = inject(Router);

  protected async logout(): Promise<void> {
    this.service.logout();
    await this.router.navigate(['/login']);
  }
}

