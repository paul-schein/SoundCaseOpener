import {inject, Injectable} from '@angular/core';
import {HttpClient, HttpErrorResponse, HttpStatusCode} from '@angular/common/http';
import {ConfigService} from './config.service';
import {z} from 'zod';
import {firstValueFrom} from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class LoginService {
  private readonly http: HttpClient = inject(HttpClient);
  private readonly configService: ConfigService = inject(ConfigService);
  private readonly baseUrl: string = `${this.configService.config.backendBaseUrl}/users`;
  private currentUser: User | null;

  public async getUserByUsername(username: string): Promise<User | null> {
    const url = `${this.baseUrl}/username/${username}`;
    try {
      let res = await firstValueFrom(this.http.get(url, {observe: 'response'}));

      const user = userZod.parse(res.body);
      this.currentUser = user;
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
      this.currentUser = user;
      return user;
    } catch (error) {
      throw new Error(`Failed to add user: ${error}`);
    }
  }

  public getCurrentUser(): User | null {
    return this.currentUser;
  }
}


const userZod = z.object({
  id: z.number().int().positive(),
  username: z.string(),
  role: z.enum(['User', 'Admin'])
});

export type User = z.infer<typeof userZod>;
