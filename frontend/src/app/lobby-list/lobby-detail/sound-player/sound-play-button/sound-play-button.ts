import {Component, computed, inject, input, InputSignal, OnInit, Signal, signal, WritableSignal} from '@angular/core';
import {MatButton} from '@angular/material/button';
import {LobbyService} from '../../../../../core/services/lobby-service';
import {Sound} from '../../../../../core/services/sound-service';
import {ChronoUnit, Instant} from '@js-joda/core';

@Component({
  selector: 'app-sound-play-button',
  imports: [
    MatButton
  ],
  templateUrl: './sound-play-button.html',
  styleUrl: './sound-play-button.scss'
})
export class SoundPlayButton implements OnInit {
  public readonly sound: InputSignal<Sound> = input.required<Sound>();
  protected readonly currentCooldown: WritableSignal<number> = signal(0);
  protected readonly btnDisabled: Signal<boolean> = computed(() => {
    return this.currentCooldown() > 0;
  });

  private readonly lobbyService = inject(LobbyService);
  private intervalId: number | null = null;

  public ngOnInit(): void {
    const sound: Sound = this.sound();
    if (sound.lastTimeUsed === null) {
      return;
    }
    const secondsSinceUse: number = ChronoUnit.SECONDS.between(sound.lastTimeUsed, Instant.now());
    if (secondsSinceUse < sound.cooldown) {
      this.startCooldown(sound.cooldown - secondsSinceUse);
    }
  }

  protected async handlePlaySound(): Promise<void> {
    const sound: Sound = this.sound();
    if (await this.lobbyService.playSound(sound.id)) {
      this.startCooldown(sound.cooldown);
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
