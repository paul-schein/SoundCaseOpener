import {inject, Injectable, signal, WritableSignal} from '@angular/core';
import {HttpClient, HttpErrorResponse, HttpStatusCode} from '@angular/common/http';
import {ConfigService} from './config-service';
import {z} from 'zod';
import {firstValueFrom} from 'rxjs';
import {RoleSchema} from '../util/zod-schemas';

@Injectable({
  providedIn: 'root'
})
export class LoginService {
  private readonly http: HttpClient = inject(HttpClient);
  private readonly configService: ConfigService = inject(ConfigService);
  private readonly baseUrl: string = `${this.configService.config.backendBaseUrl}/api/users`;
  public currentUser: WritableSignal<User | null> = signal(null);

  public async getUserByUsername(username: string): Promise<User | null> {
    const url = `${this.baseUrl}/username/${username}`;
    try {
      let res = await firstValueFrom(this.http.get(url, {observe: 'response'}));

      const user: User = userZod.parse(res.body);
      this.setCurrentUser(user);
      return user;
    } catch (error) {
     if (error instanceof HttpErrorResponse && error.status === HttpStatusCode.NotFound) {
        return null;
     }
      throw new Error(`Failed to get user: ${error}`);
    }
  }

  public async addUserByUsername(username: string): Promise<User | null> {
    const url = `${this.baseUrl}`;
    try {
      const res = await firstValueFrom(this.http.post(url, {username: username}, {observe: 'response'}));

      const user = userZod.parse(res.body);
      this.setCurrentUser(user);
      return user;
    } catch (error) {
      throw new Error(`Failed to add user: ${error}`);
    }
  }

  public logout(): void{
    localStorage.removeItem('user');
    this.currentUser.set(null);
  }

  public async tryLoadCurrentUserAsync(): Promise<boolean> {
    const userJson = localStorage.getItem('user');
    if (userJson) {
      try {
        const username: string = userZod.parse(JSON.parse(userJson)).username;
        const user: User | null = await this.getUserByUsername(username);
        if (user === null) {
          localStorage.removeItem('user');
          return false;
        }
        this.currentUser.set(user);

        return true;
      } catch (error) {
        console.error('Failed to parse current user from session storage:', error);
        this.currentUser.set(null);
      }
    } else {
      this.currentUser.set(null);
    }
    return false;
  }

  private setCurrentUser(user: User): void {
    localStorage.setItem('user', JSON.stringify(user));
    this.currentUser.set(user);
  }
}

const userZod = z.object({
  id: z.number().int().positive(),
  username: z.string(),
  role: RoleSchema
});

export type User = z.infer<typeof userZod>;
