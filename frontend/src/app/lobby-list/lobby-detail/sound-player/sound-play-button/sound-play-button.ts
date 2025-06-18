import {Component, inject, input, InputSignal, signal, WritableSignal} from '@angular/core';
import {MatButton} from '@angular/material/button';
import {LobbyService} from '../../../../../core/lobby.service';

@Component({
  selector: 'app-sound-play-button',
  imports: [
    MatButton
  ],
  templateUrl: './sound-play-button.html',
  styleUrl: './sound-play-button.scss'
})
export class SoundPlayButton {
  public readonly soundId: InputSignal<number> = input.required<number>();
  public readonly currentCooldown: WritableSignal<number> = signal(0);

  private readonly lobbyService = inject(LobbyService);
  private intervalId: number | null = null;

  public async handlePlaySound(): Promise<void> {
    await this.lobbyService.playSound(this.soundId());
    this.startCooldown(10);
  }

  private startCooldown(seconds: number) {
    this.currentCooldown.set(seconds);
    clearInterval(this.intervalId ?? 0);
    this.intervalId = setInterval(() => {
      const current = this.currentCooldown();
      if (current > 0) {
        this.currentCooldown.set(current - 1);
      } else {
        clearInterval(this.intervalId ?? 0);
      }
    }, 1000);
  }
}
