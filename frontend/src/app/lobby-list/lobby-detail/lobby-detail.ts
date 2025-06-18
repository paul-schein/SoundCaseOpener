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
import {Lobby, LobbyService} from '../../../core/lobby.service';
import {Router} from '@angular/router';
import {Subscription} from 'rxjs';
import {LobbyUserCount} from '../lobby-user-count/lobby-user-count';

@Component({
  selector: 'app-lobby-detail',
  imports: [
    LobbyUserCount
  ],
  templateUrl: './lobby-detail.html',
  styleUrl: './lobby-detail.scss'
})
export class LobbyDetail implements OnInit, OnDestroy {
  public lobbyId: InputSignal<string> = input.required<string>();
  public isCreator: InputSignalWithTransform<boolean, string> = input(false, {
    transform: (value: string) => value === 'true'
  });
  protected users: WritableSignal<string[]> = signal([]);
  protected lobby: WritableSignal<Lobby | null> = signal(null);

  private readonly lobbyService = inject(LobbyService);
  private readonly router: Router = inject(Router);
  private readonly subscriptions: Subscription[] = [];

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
      })
    );
  }

  public async ngOnDestroy(): Promise<void> {
    await this.lobbyService.leaveLobby(this.lobbyId());
    this.subscriptions.forEach(subscription => subscription.unsubscribe());
  }
}
