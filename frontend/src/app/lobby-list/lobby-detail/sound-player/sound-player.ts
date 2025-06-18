import {Component, inject} from '@angular/core';
import {LobbyService} from '../../../../core/services/lobby-service';
import {SoundPlayButton} from './sound-play-button/sound-play-button';

@Component({
  selector: 'app-sound-player',
  imports: [
    SoundPlayButton
  ],
  templateUrl: './sound-player.html',
  styleUrl: './sound-player.scss'
})
export class SoundPlayer {

}
