import {Component, input, InputSignal} from '@angular/core';
import {MatCard, MatCardHeader, MatCardSubtitle, MatCardTitle} from '@angular/material/card';
import {Lobby} from '../../../core/lobby.service';
import {LobbyUserCount} from '../lobby-user-count/lobby-user-count';

@Component({
  selector: 'app-lobby-card',
  imports: [
    MatCard,
    MatCardHeader,
    MatCardTitle,
    MatCardSubtitle,
    LobbyUserCount
  ],
  templateUrl: './lobby-card.html',
  styleUrl: './lobby-card.scss'
})
export class LobbyCard {
  public lobby: InputSignal<Lobby> = input.required<Lobby>();
}
