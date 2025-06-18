import {Component, inject} from '@angular/core';
import {LoginService} from '../../core/login.service';
import {Router, RouterLink} from '@angular/router';
import {MatButton} from '@angular/material/button';
import {Home} from '../home/home';


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
  protected readonly isAdmin: boolean = false;
  private readonly service: LoginService = inject(LoginService);
  private readonly router: Router = inject(Router);

  constructor() {
    this.isAdmin = this.service.currentUser()?.role === 'Admin';
  }

  protected readonly Home = Home;

  protected async logout(): Promise<void> {
    this.service.logout();
    await this.router.navigate(['/login']);
  }
}

