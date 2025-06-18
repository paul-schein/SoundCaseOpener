import { Component, effect, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
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

  protected readonly points = this.generateSineWave();

  private generateSineWave(): string {
    const width = 1000; // SVG width
    const height = 200;  // SVG height
    const points: string[] = [];
    const segments = 100;

    for (let i = 0; i <= segments; i++) {
      const x = (i / segments) * width;
      const y = height / 2 + Math.sin(i * 0.2) * 50;
      points.push(`${x},${y}`);
    }

    return points.join(' ');
  }
}
