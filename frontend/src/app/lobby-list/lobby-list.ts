import {
  Component,
  computed,
  inject, model, ModelSignal,
  OnDestroy,
  OnInit,
  Signal,
  signal,
  WritableSignal
} from '@angular/core';
import {Lobby, LobbyService} from '../../core/lobby.service';
import {Subscription} from 'rxjs';
import {MatProgressBar} from '@angular/material/progress-bar';
import {LobbyCard} from './lobby-card/lobby-card';
import {MatButton} from '@angular/material/button';
import {Router, RouterLink} from '@angular/router';
import {MatInput} from '@angular/material/input';
import {MatFormField, MatLabel} from '@angular/material/form-field';
import {FormsModule} from '@angular/forms';

@Component({
  selector: 'app-lobby-list',
  imports: [
    MatProgressBar,
    LobbyCard,
    MatButton,
    RouterLink,
    MatFormField,
    MatLabel,
    FormsModule,
    MatInput
  ],
  templateUrl: './lobby-list.html',
  styleUrl: './lobby-list.scss'
})
export class LobbyList implements OnInit, OnDestroy {
  protected readonly lobbies: WritableSignal<Lobby[]> = signal([]);
  protected readonly isLoading: WritableSignal<boolean> = signal(true);
  protected readonly noLobbies: Signal<boolean> = computed(() =>
    !(this.isLoading()) && this.lobbies().length === 0);
  protected readonly newLobbyName: ModelSignal<string> = model<string>("");
  protected readonly createBtnDisabled: Signal<boolean> = computed(() => {
    const name: string | undefined = this.newLobbyName();
    if (name === undefined) {
      return true;
    }
    return name.trim() === "" || this.isLoading();
  });
  protected readonly filter: ModelSignal<string> = model<string>("");
  protected readonly filteredLobbies: Signal<Lobby[]> = computed(() => {
    let filter: string | undefined = this.filter();
    if (filter === undefined) {
      return this.lobbies();
    }
    filter = filter.trim().toLowerCase();
    return this.lobbies().filter(lobby =>
      lobby.name.toLowerCase().includes(filter));
  });

  private readonly lobbyService: LobbyService = inject(LobbyService);
  private readonly subscriptions: Subscription[] = [];
  private readonly router = inject(Router);

  protected async handleCreateLobby(): Promise<void> {
    const lobby: Lobby | null = await this.lobbyService.createLobby(this.newLobbyName());
    if (lobby === null) {
      console.error('Failed to create lobby');
      return;
    }
    await this.router.navigate(['/lobby', lobby.id], {
      queryParams: {isCreator: 'true'}
    });
  }

  public async ngOnInit(): Promise<void> {
    await this.lobbyService.initializeConnection();
    this.lobbies.set(await this.lobbyService.getLobbies());
    this.subscriptions.push(
      this.lobbyService.lobbyCreated$.subscribe(lobby => {
        this.lobbies.update(lobbies => [...lobbies, lobby]);
      }),
      this.lobbyService.lobbyClosed$.subscribe(lobbyId => {
        this.lobbies.update(lobbies =>
          lobbies.filter(lobby => lobby.id !== lobbyId));
      }),
      this.lobbyService.lobbyUserCountChange$.subscribe(change => {
        this.lobbies.update(lobbies =>
          lobbies.map(lobby => {
            if (lobby.id === change.lobbyId) {
              lobby.userCount += change.deltaCount;
            }
            return lobby;
          }));
      }));
    this.newLobbyName.set("New Lobby");
    this.isLoading.set(false);
  }

  public async ngOnDestroy(): Promise<void> {
    this.subscriptions.forEach(s => s.unsubscribe());
  }
}
