import {Component, input, InputSignal} from '@angular/core';
import {MatCard, MatCardHeader, MatCardSubtitle, MatCardTitle} from '@angular/material/card';
import {Lobby} from '../../../core/lobby.service';

@Component({
  selector: 'app-lobby-card',
  imports: [
    MatCard,
    MatCardHeader,
    MatCardTitle,
    MatCardSubtitle
  ],
  templateUrl: './lobby-card.html',
  styleUrl: './lobby-card.scss'
})
export class LobbyCard {
  public lobby: InputSignal<Lobby> = input.required<Lobby>();
}
