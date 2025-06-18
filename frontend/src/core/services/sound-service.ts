import {inject, Injectable} from '@angular/core';
import {ServiceBase} from './service-base';
import {z} from 'zod';
import {ConfigService} from '../config.service';
import {RaritySchema} from '../util/zod-schemas';
import {lastValueFrom} from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class SoundService extends ServiceBase {

  protected override get controller(): string {
    return 'case-templates';
  }

  public async addSoundTemplate(newSoundTemplate: NewSoundTemplate): Promise<SoundTemplate | undefined> {
    const url = this.buildUrl(null);

    try {
      const response = await lastValueFrom(
        this.http.post<any>(url, newSoundTemplate, {
          observe: 'response'
        })
      );

      return soundTemplateSchema.parse(response.body);
    } catch (error: any) {
      console.error(`Error adding sound template: ${JSON.stringify(error)}`);
      return undefined;
    }
  }
}

const newSoundTemplateSchema = z.object({
  name: z.string().min(4),
  description: z.string().max(200),
  rarity: RaritySchema,
  minCooldown: z.number().int(),
  maxCooldown: z.number().int(),
  soundFileId: z.number().int(),
})

export type NewSoundTemplate = z.infer<typeof newSoundTemplateSchema>;

const soundTemplateSchema = newSoundTemplateSchema.extend({
  id: z.number().nonnegative(),
});

export type SoundTemplate = z.infer<typeof soundTemplateSchema>;
