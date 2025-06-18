import { inject, Injectable } from '@angular/core';
import { ServiceBase } from './service-base';
import { z } from 'zod';
import { RaritySchema } from '../util/zod-schemas';
import { LoginService } from './login-service';
import { firstValueFrom } from 'rxjs';
import { Sound, soundZod } from './sound-service';

@Injectable({
  providedIn: 'root'
})
export class CaseService extends ServiceBase {
  private readonly loginService: LoginService = inject(LoginService);

  protected override get controller(): string {
    return 'cases';
  }

  public async getAllCasesOfUser(): Promise<Case[]> {
    const url = this.buildUrl(`user/${this.loginService.currentUser()?.id}`);
    try {
      const response = await firstValueFrom(
        this.http.get<AllCasesOfUserResponse>(url, { observe: 'response' }));
      return caseListResponseZod.parse(response.body).cases;
    } catch (error) {
      console.error(`Error getting cases: ${JSON.stringify(error)}`);
      return [];
    }
  }

  public async getCaseById(caseId: number): Promise<Case | null> {
    if (caseId <= 0) return null;

    const url = this.buildUrl(`${caseId}`);
    try {
      const response = await firstValueFrom(
        this.http.get<Case>(url, { observe: 'response' }));
      return caseZod.parse(response.body);
    } catch (error) {
      console.error(`Error getting case: ${JSON.stringify(error)}`);
      return null;
    }
  }

  public async changeCaseName(caseId: number, newName: string): Promise<Case | null> {
    if (caseId <= 0 || !newName?.trim()) return null;

    const url = this.buildUrl(`${caseId}/name/${encodeURIComponent(newName)}`);
    try {
      const response = await firstValueFrom(
        this.http.patch<Case>(url, null, { observe: 'response' }));
      return caseZod.parse(response.body);
    } catch (error) {
      console.error(`Error changing case name: ${JSON.stringify(error)}`);
      return null;
    }
  }

  public async openCase(caseId: number): Promise<Sound | null> {
    if (caseId <= 0) return null;

    const url = this.buildUrl(`${caseId}/open`);
    try {
      const response = await firstValueFrom(
        this.http.post<Sound>(url, null, { observe: 'response' }));
      return soundZod.parse(response.body);
    } catch (error) {
      console.error(`Error opening case: ${JSON.stringify(error)}`);
      return null;
    }
  }
}

const caseZod = z.object({
  id: z.number().int().positive(),
  name: z.string().nonempty(),
  description: z.string().nonempty(),
  rarity: RaritySchema
});

export type Case = z.infer<typeof caseZod>;

const caseListResponseZod = z.object({
  cases: caseZod.array()
});

export type AllCasesOfUserResponse = z.infer<typeof caseListResponseZod>;

export enum CaseErrorType {
  NotFound = 'NOT_FOUND',
  InvalidInput = 'INVALID_INPUT',
  ServerError = 'SERVER_ERROR'
}

export class CaseError extends Error {
  constructor(
    public readonly type: CaseErrorType,
    message: string
  ) {
    super(message);
    this.name = 'CaseError';
  }
}
