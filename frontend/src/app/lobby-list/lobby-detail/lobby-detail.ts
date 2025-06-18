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
import {LobbyService} from '../../../core/lobby.service';
import {Router} from '@angular/router';
import {Subscription} from 'rxjs';

@Component({
  selector: 'app-lobby-detail',
  imports: [],
  templateUrl: './lobby-detail.html',
  styleUrl: './lobby-detail.scss'
})
export class LobbyDetail implements OnInit, OnDestroy {
  public lobbyId: InputSignal<string> = input.required<string>();
  public isCreator: InputSignalWithTransform<boolean, string> = input(false, {
    transform: (value: string) => value === 'true'
  });
  protected users: WritableSignal<string[]> = signal([]);

  private readonly lobbyService = inject(LobbyService);
  private readonly router: Router = inject(Router);
  private readonly subscriptions: Subscription[] = [];

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
    this.subscriptions.push(
      this.lobbyService.userJoinedLobby$.subscribe((users: string) => {
        this.users.update((currentUsers: string[]) => [...currentUsers, users]);
      }),
      this.lobbyService.userLeftLobby$.subscribe((users: string) => {
        this.users.update((currentUsers: string[]) => currentUsers.filter(user => user !== users));
      })
    );
  }

  public async ngOnDestroy(): Promise<void> {
    await this.lobbyService.leaveLobby(this.lobbyId());
    this.subscriptions.forEach(subscription => subscription.unsubscribe());
  }
}
