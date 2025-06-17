import {Component, inject} from '@angular/core';
import {FormBuilder, FormGroup, ReactiveFormsModule, Validators} from '@angular/forms';
import {MatFormField, MatInput, MatLabel, MatError} from '@angular/material/input';
import {MatButton} from '@angular/material/button';
import {LoginService} from '../../core/login.service';
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
export class Login {
  private readonly loginService: LoginService = inject(LoginService);
  private readonly router: Router = inject(Router);
  private readonly formBuilder: FormBuilder = inject(FormBuilder);
  protected readonly formGroup: FormGroup = this.formBuilder.group({
    username: ['', [Validators.required]],
  });

  protected async onSubmit(){
    try{
      const usr= this.formGroup.get('username')?.value;
      let res = await this.loginService.getUserByUsername(usr);
      if(res == null){
        let added = await this.loginService.addUserByUsername(usr);
        sessionStorage.setItem('user', JSON.stringify(added));
      }
      await this.router.navigate(['']);
    }catch(err){
      console.error(err);
    }
  }
}
