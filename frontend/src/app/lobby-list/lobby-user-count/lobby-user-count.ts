import {Component, input, InputSignal} from '@angular/core';
import {Lobby} from '../../../core/lobby.service';
import {MatIcon} from '@angular/material/icon';

@Component({
  selector: 'app-lobby-user-count',
  imports: [
    MatIcon
  ],
  templateUrl: './lobby-user-count.html',
  styleUrl: './lobby-user-count.scss'
})
export class LobbyUserCount {
  public readonly lobby: InputSignal<Lobby> = input.required<Lobby>();
}
