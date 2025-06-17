import {Component, computed, inject, OnDestroy, OnInit, Signal, signal, WritableSignal} from '@angular/core';
import {Lobby, LobbyService} from '../../core/lobby.service';
import {Subscription} from 'rxjs';
import {MatProgressBar} from '@angular/material/progress-bar';
import {LobbyCard} from './lobby-card/lobby-card';
import {MatButton} from '@angular/material/button';
import {Router} from '@angular/router';

@Component({
  selector: 'app-lobby-list',
  imports: [
    MatProgressBar,
    LobbyCard,
    MatButton
  ],
  templateUrl: './lobby-list.html',
  styleUrl: './lobby-list.scss'
})
export class LobbyList implements OnInit, OnDestroy {
  protected readonly lobbies: WritableSignal<Lobby[]> = signal([]);
  protected readonly isLoading: WritableSignal<boolean> = signal(true);
  protected readonly noLobbies: Signal<boolean> = computed(() =>
    !(this.isLoading()) && this.lobbies().length === 0);

  private readonly lobbyService: LobbyService = inject(LobbyService);
  private readonly subscriptions: Subscription[] = [];
  private readonly router = inject(Router);

  protected async handleCreateLobby(): Promise<void> {
    const lobby: Lobby = await this.lobbyService.createLobby("test");
    await this.router.navigate(['/lobby', lobby.id]);
  }

  public async ngOnInit(): Promise<void> {
    await this.lobbyService.initializeConnection();
    this.lobbies.set(await this.lobbyService.getLobbies());
    this.subscriptions.push(this.lobbyService.lobbyCreated$.subscribe(lobby => {
      this.lobbies.update(lobbies => [...lobbies, lobby]);
    }));
    this.subscriptions.push(this.lobbyService.lobbyClosed$.subscribe(lobbyId => {
      this.lobbies.update(lobbies =>
        lobbies.filter(lobby => lobby.id !== lobbyId));
    }));
    this.isLoading.set(false);
  }

  public async ngOnDestroy(): Promise<void> {
    this.subscriptions.forEach(s => s.unsubscribe());
  }
}
