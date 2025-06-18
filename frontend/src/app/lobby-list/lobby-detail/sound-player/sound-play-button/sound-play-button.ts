import {Component, computed, inject, input, InputSignal, Signal, signal, WritableSignal} from '@angular/core';
import {MatButton} from '@angular/material/button';
import {LobbyService} from '../../../../../core/services/lobby-service';

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
  protected readonly currentCooldown: WritableSignal<number> = signal(0);
  protected readonly btnDisabled: Signal<boolean> = computed(() => {
    return this.currentCooldown() > 0;
  });

  private readonly lobbyService = inject(LobbyService);
  private intervalId: number | null = null;

  protected async handlePlaySound(): Promise<void> {
    if (await this.lobbyService.playSound(this.soundId()) || true) { // remove true when sound object is available
      this.startCooldown(10);
    }
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
