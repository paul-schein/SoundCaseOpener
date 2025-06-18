import {Component, computed, inject, OnInit, Signal, signal, WritableSignal} from '@angular/core';
import {LobbyService} from '../../../../core/services/lobby-service';
import {SoundPlayButton} from './sound-play-button/sound-play-button';
import {Sound, SoundService} from '../../../../core/services/sound-service';

@Component({
  selector: 'app-sound-player',
  imports: [
    SoundPlayButton
  ],
  templateUrl: './sound-player.html',
  styleUrl: './sound-player.scss'
})
export class SoundPlayer implements OnInit {
  protected readonly sounds: WritableSignal<Sound[]> = signal([]);
  protected readonly noSounds: Signal<boolean> = computed(() => this.sounds().length === 0);

  private readonly soundService: SoundService = inject(SoundService);

  public async ngOnInit(): Promise<void> {
    this.sounds.set(await this.soundService.getAllSoundsOfUser());
  }
}
