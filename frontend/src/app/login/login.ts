import {Component, inject, OnInit, signal, WritableSignal} from '@angular/core';
import {FormBuilder, FormGroup, ReactiveFormsModule, Validators} from '@angular/forms';
import {MatFormField, MatInput, MatLabel, MatError} from '@angular/material/input';
import {MatButton} from '@angular/material/button';
import {LoginService, User} from '../../core/services/login-service';
import {Router} from '@angular/router';

@Component({
  selector: 'app-login',
  imports: [
    ReactiveFormsModule,
    MatInput,
    MatFormField,
    MatError,
    MatLabel,
    MatButton
  ],
  templateUrl: './login.html',
  styleUrl: './login.scss'
})
export class Login implements OnInit {
  private readonly loginService: LoginService = inject(LoginService);
  private readonly router: Router = inject(Router);
  private readonly formBuilder: FormBuilder = inject(FormBuilder);
  protected readonly formGroup: FormGroup = this.formBuilder.group({
    username: ['', [Validators.required]],
  });
  protected readonly isLoggedIn: WritableSignal<boolean> = signal(true);

  protected async onSubmit(){
    try{
      const usr= this.formGroup.get('username')?.value;
      let res: User | null = await this.loginService.getUserByUsername(usr);
      if(res == null){
        res = await this.loginService.addUserByUsername(usr);
      }
      await this.router.navigate(['home']);
    }catch(err){
      console.error(err);
    }
  }

  public async ngOnInit(): Promise<void> {
    if (this.loginService.currentUser() !== null
    || await this.loginService.tryLoadCurrentUserAsync()) {
      await this.router.navigate(['home']);
    }
    this.isLoggedIn.set(false);
  }
}
