import {inject, Injectable } from '@angular/core';
import {ServiceBase} from './service-base'
import { z } from 'zod';
import {RaritySchema} from '../util/zod-schemas';
import {LoginService} from './login-service';
import {firstValueFrom} from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class SoundService extends ServiceBase {
  private readonly loginService: LoginService = inject(LoginService);

  protected override get controller(): string {
    return 'sounds';
  }

  public async getAllSoundsOfUser(): Promise<Sound[]> {
    const url = this.buildUrl(`user/${this.loginService.currentUser()?.id}`);
    try {
      const response = await firstValueFrom(
        this.http.get<SoundListResponse>(url, { observe: 'response' }));
      return soundListResponseZod.parse(response.body).sounds;
    } catch (error) {
      console.error(`Error getting sounds: ${JSON.stringify(error)}`);
      return [];
    }
  }
}

const soundZod = z.object({
  id: z.number().int().nonnegative(),
  name: z.string().nonempty(),
  description: z.string().nonempty().max(200),
  rarity: RaritySchema,
  cooldown: z.number().int().nonnegative(),
  filePath: z.string().nonempty()
});

export type Sound = z.infer<typeof soundZod>;

const soundListResponseZod = z.object({
  sounds: soundZod.array()
});

export type SoundListResponse = z.infer<typeof soundListResponseZod>;
