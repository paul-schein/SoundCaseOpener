import {
  Component,
  inject,
  input,
  InputSignal,
  InputSignalWithTransform,
  OnDestroy,
  OnInit, signal,
  WritableSignal
} from '@angular/core';
import {Lobby, LobbyService} from '../../../core/services/lobby-service';
import {Router} from '@angular/router';
import {Subscription} from 'rxjs';
import {LobbyUserCount} from '../lobby-user-count/lobby-user-count';
import {ConfigService} from '../../../core/services/config-service';
import {SoundPlayer} from './sound-player/sound-player';
import {NgClass} from '@angular/common';
import {SnackbarService} from '../../../core/services/snackbar-service';

@Component({
  selector: 'app-lobby-detail',
  imports: [
    LobbyUserCount,
    SoundPlayer,
    NgClass
  ],
  templateUrl: './lobby-detail.html',
  styleUrl: './lobby-detail.scss'
})
export class LobbyDetail implements OnInit, OnDestroy {
  public readonly lobbyId: InputSignal<string> = input.required<string>();
  public readonly isCreator: InputSignalWithTransform<boolean, string> = input(false, {
    transform: (value: string) => value === 'true'
  });
  protected readonly users: WritableSignal<string[]> = signal([]);
  protected readonly lobby: WritableSignal<Lobby | null> = signal(null);
  protected readonly usersWithSound: WritableSignal<string[]> = signal([]);

  private readonly configService = inject(ConfigService);
  private readonly lobbyService = inject(LobbyService);
  private readonly router: Router = inject(Router);
  private readonly snackbar: SnackbarService = inject(SnackbarService);
  private readonly subscriptions: Subscription[] = [];
  private readonly audios: HTMLAudioElement[] = [];

  protected async handleLeaveLobby(): Promise<void> {
    await this.lobbyService.leaveLobby(this.lobbyId());
    await this.router.navigate(['/lobby-list']);
  }

  public async ngOnInit(): Promise<void> {
    await this.lobbyService.initializeConnection();
    if (!this.isCreator()) {
      await this.lobbyService.joinLobby(this.lobbyId());
    }
    else {
      await this.router.navigate([], {
        relativeTo: this.router.routerState.root,
        queryParams: {},
        replaceUrl: true
      });
    }

    this.users.set(await this.lobbyService.getUsersInLobby(this.lobbyId()));
    this.lobby.set(await this.lobbyService.getLobbyById(this.lobbyId()));
    this.subscriptions.push(
      this.lobbyService.userJoinedLobby$.subscribe((users: string) => {
        this.users.update((currentUsers: string[]) => [...currentUsers, users]);
        this.lobby.update((lobby: Lobby | null) => {
          if (lobby) {
            lobby.userCount++;
          }
          return lobby;
        });
      }),
      this.lobbyService.userLeftLobby$.subscribe((users: string) => {
        this.users.update((currentUsers: string[]) => currentUsers.filter(user => user !== users));
        this.lobby.update((lobby: Lobby | null) => {
          if (lobby) {
            lobby.userCount--;
          }
          return lobby;
        });
      }),
      this.lobbyService.userPlayedSound$.subscribe(async soundPlayed => {
        await this.handleSoundPlayed(soundPlayed.username, soundPlayed.filePath);
      }),
      this.lobbyService.caseObtained$.subscribe(caseObtained => {
        this.snackbar.show("You obtained a new case! You can open it in your inventory.");
      })
    );
  }

  private async handleSoundPlayed(username: string, filePath: string): Promise<void> {
    const audio = new Audio(`${this.configService.config.soundsBaseUrl}/${filePath}`);
    this.audios.push(audio);
    await audio.play();
    if (!this.usersWithSound().includes(username)) {
      this.usersWithSound.update(current => [...current, username]);
      setTimeout(() => {
        this.usersWithSound.update(current => current.filter(user => user !== username));
      }, audio.duration * 1000);
    }
  }

  public async ngOnDestroy(): Promise<void> {
    this.audios.forEach(audio => {
      audio.pause();
      audio.currentTime = 0;
    });
    await this.lobbyService.leaveLobby(this.lobbyId());
    this.subscriptions.forEach(subscription => subscription.unsubscribe());
  }
}
