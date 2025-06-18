import {inject, Injectable} from '@angular/core';
import {ServiceBase} from './service-base';
import {z} from 'zod';
import {ConfigService} from './config-service';
import {RaritySchema} from '../util/zod-schemas';
import {firstValueFrom, lastValueFrom} from 'rxjs';
import {SoundTemplateListResponse} from './sound-template-service';

@Injectable({
  providedIn: 'root'
})
export class CaseTemplateService extends ServiceBase {

  protected override get controller(): string {
    return 'case-templates';
  }

  public async getAllCaseTemplates(): Promise<CaseTemplateListResponse | undefined> {
    const url = this.buildUrl(null);
    try {
      const response = await firstValueFrom(this.http.get<SoundTemplateListResponse>(url, { observe: "response" }));

      const data = caseTemplateListResponseSchema.parse(response.body);
      return data as CaseTemplateListResponse;
    } catch (error) {
      console.log(`Error getting Case Templates: ${JSON.stringify(error)}`);
      return undefined;
    }
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

  public async addToCaseTemplate(newItemCaseToCaseTemplate: NewItemTemplateToCaseTemplate): Promise<boolean> {
    const url = this.buildUrl(`add-item-template`);

    try {
      const response = await lastValueFrom(
        this.http.post<any>(url, newItemCaseToCaseTemplate, {
          observe: 'response'
        })
      );

      return true;
    } catch (error: any) {
      console.error(`Error adding case template: ${JSON.stringify(error)}`);
      return false;
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

const caseTemplateListResponseSchema = z.object({
  caseTemplates: caseTemplateSchema.array()
})

export type CaseTemplateListResponse = z.infer<typeof caseTemplateListResponseSchema>;

const newItemTemplateToCaseTemplateSchema = z.object({
  caseTemplateId: z.number().nonnegative(),
  itemTemplateId: z.number().nonnegative(),
  weight: z.number().nonnegative(),
})

export type NewItemTemplateToCaseTemplate = z.infer<typeof newItemTemplateToCaseTemplateSchema>;
