import { Injectable, signal } from '@angular/core';
import { Case } from '../../../core/services/case-service';
import { Sound } from '../../../core/services/sound-service';

export interface CaseOpeningState {
  case: Case;
  progress: number;
  obtainedSound?: Sound;
  animationPhase: 'initial' | 'spinning' | 'revealing' | 'complete';
}

@Injectable({
  providedIn: 'root'
})
export class CaseOpeningStateService {
  private readonly openingState = signal<CaseOpeningState | null>(null);
  public readonly openingState$ = this.openingState.asReadonly();

  startOpening(case_: Case) {
    this.openingState.set({
      case: case_,
      progress: 0,
      animationPhase: 'initial'
    });

    setTimeout(() => {
      this.setAnimationPhase('spinning');
      this.animateProgress(0, 100, 3000);
    }, 500);

    setTimeout(() => this.setAnimationPhase('revealing'), 3600);
    setTimeout(() => this.setAnimationPhase('complete'), 5600);
  }

  private setAnimationPhase(phase: CaseOpeningState['animationPhase']) {
    const current = this.openingState();
    if (current) {
      this.openingState.set({ ...current, animationPhase: phase });
    }
  }

  private animateProgress(from: number, to: number, duration: number) {
    const start = Date.now();
    const step = () => {
      const now = Date.now();
      const elapsed = now - start;
      let next = from + ((to - from) * (elapsed / duration));
      next = Math.min(next, to);
      this.updateProgress(next);
      if (elapsed < duration) {
        requestAnimationFrame(step);
      } else {
        this.updateProgress(to);
      }
    };
    step();
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
