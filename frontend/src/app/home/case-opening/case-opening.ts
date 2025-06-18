import { Component, EventEmitter, Output, effect, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { Sound } from '../../../core/services/sound-service';
import {CaseOpeningStateService} from '../util/case-opening-state-service';

@Component({
  selector: 'app-case-opening',
  standalone: true,
  imports: [CommonModule, MatCardModule, MatIconModule],
  templateUrl: './case-opening.html',
  styleUrl: './case-opening.scss'
})
export class CaseOpeningComponent {
  private readonly state = inject(CaseOpeningStateService);
  protected readonly openingState = this.state.openingState$;
  @Output() soundObtained = new EventEmitter<Sound>();

  private readonly ANIMATION_DURATION = 3000; // 3 seconds
  private animationFrame: number | null = null;

  constructor() {
    effect(() => {
      const state = this.openingState();
      if (state && !state.obtainedSound) {
        this.startAnimation();
      }
    });
  }

  private startAnimation() {
    const startTime = performance.now();

    const animate = (currentTime: number) => {
      const elapsed = currentTime - startTime;
      const progress = Math.min(elapsed / this.ANIMATION_DURATION, 1);

      // Sine wave animation progress
      const wave = Math.sin(progress * Math.PI * 4) * Math.cos(progress * Math.PI * 2);
      const normalizedWave = (wave + 1) / 2; // Convert from [-1, 1] to [0, 1]

      this.state.updateProgress(normalizedWave);

      if (progress < 1) {
        this.animationFrame = requestAnimationFrame(animate);
      } else {
        this.animationFrame = null;
      }
    };

    this.animationFrame = requestAnimationFrame(animate);
  }

  ngOnDestroy() {
    if (this.animationFrame !== null) {
      cancelAnimationFrame(this.animationFrame);
    }
  }
}
