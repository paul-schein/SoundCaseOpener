import { Injectable, signal } from '@angular/core';
import {Case} from '../../../core/services/case-service';
import {Sound} from '../../../core/services/sound-service';


export interface CaseOpeningState {
  case: Case;
  progress: number;
  obtainedSound?: Sound;
}

@Injectable({
  providedIn: 'root'
})
export class CaseOpeningStateService {
  private readonly openingState = signal<CaseOpeningState | null>(null);
  public readonly openingState$ = this.openingState.asReadonly();

  startOpening(case_: Case) {
    this.openingState.set({ case: case_, progress: 0 });
  }

  updateProgress(progress: number) {
    const current = this.openingState();
    if (current) {
      this.openingState.set({ ...current, progress });
    }
  }

  setObtainedSound(sound: Sound) {
    const current = this.openingState();
    if (current) {
      this.openingState.set({ ...current, obtainedSound: sound });
    }
  }

  reset() {
    this.openingState.set(null);
  }
}
