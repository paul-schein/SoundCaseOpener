import { inject, Injectable } from '@angular/core';
import { ServiceBase } from './service-base';
import { z } from 'zod';
import { RaritySchema } from '../util/zod-schemas';
import { LoginService } from './login-service';
import { firstValueFrom } from 'rxjs';
import { soundZod } from './sound-service';

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
        this.http.get<CaseListResponse>(url, { observe: 'response' }));
      return caseListResponseZod.parse(response.body).cases;
    } catch (error) {
      console.error(`Error getting cases: ${JSON.stringify(error)}`);
      return [];
    }
  }

  public async getCaseDetails(caseId: number): Promise<CaseDetails | null> {
    const url = this.buildUrl(`${caseId}/details`);
    try {
      const response = await firstValueFrom(
        this.http.get<CaseDetailsResponse>(url, { observe: 'response' }));
      return caseDetailsResponseZod.parse(response.body).caseDetails;
    } catch (error) {
      console.error(`Error getting case details: ${JSON.stringify(error)}`);
      return null;
    }
  }

  public async openCase(caseId: number): Promise<CaseOpeningResult | null> {
    const url = this.buildUrl(`${caseId}/open`);
    try {
      const response = await firstValueFrom(
        this.http.post<CaseOpeningResultResponse>(url, null, { observe: 'response' }));
      return caseOpeningResultResponseZod.parse(response.body).result;
    } catch (error) {
      console.error(`Error opening case: ${JSON.stringify(error)}`);
      return null;
    }
  }
}

const caseZod = z.object({
  id: z.number().int().nonnegative(),
  name: z.string().nonempty(),
  description: z.string().nonempty(),
  rarity: RaritySchema,
  imageUrl: z.string().nonempty(),
  price: z.number().nonnegative()
});

export type Case = z.infer<typeof caseZod>;

const caseListResponseZod = z.object({
  cases: caseZod.array()
});

export type CaseListResponse = z.infer<typeof caseListResponseZod>;

const casePossibleRewardZod = z.object({
  sound: soundZod,
  dropRate: z.number().min(0).max(100)
});

export type CasePossibleReward = z.infer<typeof casePossibleRewardZod>;

const caseDetailsZod = z.object({
  case: caseZod,
  possibleRewards: casePossibleRewardZod.array()
});

export type CaseDetails = z.infer<typeof caseDetailsZod>;

const caseDetailsResponseZod = z.object({
  caseDetails: caseDetailsZod
});

export type CaseDetailsResponse = z.infer<typeof caseDetailsResponseZod>;

const caseOpeningResultZod = z.object({
  caseId: z.number().int().nonnegative(),
  obtainedSound: soundZod,
  animationDuration: z.number().positive(),
  specialEffects: z.enum(['None', 'Rare', 'Epic', 'Legendary']).optional()
});

export type CaseOpeningResult = z.infer<typeof caseOpeningResultZod>;

const caseOpeningResultResponseZod = z.object({
  result: caseOpeningResultZod
});

export type CaseOpeningResultResponse = z.infer<typeof caseOpeningResultResponseZod>;

export enum CaseErrorType {
  NotFound = 'CASE_NOT_FOUND',
  InsufficientFunds = 'INSUFFICIENT_FUNDS',
  AlreadyOpening = 'ALREADY_OPENING_CASE',
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
