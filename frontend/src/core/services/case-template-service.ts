import {inject, Injectable} from '@angular/core';
import {ServiceBase} from './service-base';
import {z} from 'zod';
import {ConfigService} from './config-service';
import {RaritySchema} from '../util/zod-schemas';
import {lastValueFrom} from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class CaseTemplateService extends ServiceBase {

  protected override get controller(): string {
    return 'case-templates';
  }

  public async addCaseTemplate(newCaseTemplate: NewCaseTemplate): Promise<CaseTemplate | undefined> {
    const url = this.buildUrl(null);

    try {
      const response = await lastValueFrom(
        this.http.post<any>(url, newCaseTemplate, {
          observe: 'response'
        })
      );

      return caseTemplateSchema.parse(response.body);
    } catch (error: any) {
      console.error(`Error adding case template: ${JSON.stringify(error)}`);
      return undefined;
    }
  }
}

const newCaseTemplateSchema = z.object({
  name: z.string().min(4),
  description: z.string().max(200),
  rarity: RaritySchema,
})

export type NewCaseTemplate = z.infer<typeof newCaseTemplateSchema>;

const caseTemplateSchema = newCaseTemplateSchema.extend({
  id: z.number().nonnegative()
});

export type CaseTemplate = z.infer<typeof caseTemplateSchema>;
