import {Component, computed, inject, Signal} from '@angular/core';
import {RouterOutlet} from '@angular/router';
import {Navbar} from './navbar/navbar';
import {LoginService} from '../core/login.service';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, Navbar],
  templateUrl: './app.html',
  styleUrl: './app.scss'
})
export class App {
  private readonly loginService: LoginService = inject(LoginService);
  protected readonly isLoggedIn: Signal<boolean> = computed(() => {
    return this.loginService.currentUser() !== null;
  });
}
