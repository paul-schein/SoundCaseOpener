import {inject, Injectable} from '@angular/core';
import {ServiceBase} from './service-base';
import {z} from 'zod';
import {ConfigService} from '../config.service';
import {RaritySchema} from '../util/zod-schemas';
import {firstValueFrom, lastValueFrom} from 'rxjs';
import {SoundFileListResponse} from './sound-file-service';

@Injectable({
  providedIn: 'root'
})
export class SoundTemplateService extends ServiceBase {

  protected override get controller(): string {
    return 'sound-templates';
  }

  public async getAllSoundTemplates(): Promise<SoundTemplateListResponse | undefined> {
    const url = this.buildUrl(null);
    try {
      const response = await firstValueFrom(this.http.get<SoundTemplateListResponse>(url, { observe: "response" }));

      const data = soundTemplateListResponseSchema.parse(response.body);
      return data as SoundTemplateListResponse;
    } catch (error) {
      console.log(`Error getting Sound Templates: ${JSON.stringify(error)}`);
      return undefined;
    }
  }

  public async addSoundTemplate(newSoundTemplate: NewSoundTemplate): Promise<SoundTemplateResponse | undefined> {
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

export type SoundTemplateResponse = z.infer<typeof soundTemplateSchema>;

const soundTemplateListResponseSchema = z.object({
  soundTemplates: soundTemplateSchema.array()
})

export type SoundTemplateListResponse = z.infer<typeof soundTemplateListResponseSchema>;
