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
}
