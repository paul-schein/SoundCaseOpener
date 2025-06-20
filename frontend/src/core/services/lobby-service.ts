import {inject, Injectable} from '@angular/core';
import {ConfigService} from './config-service';
import {LoginService} from './login-service';
import * as signalR from '@microsoft/signalr';
import {z} from 'zod';
import {Subject} from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class LobbyService {
  private readonly configService: ConfigService = inject(ConfigService);
  private readonly loginService: LoginService = inject(LoginService);
  private initialized: boolean = false;
  private initializing: boolean = false;
  private connection: signalR.HubConnection = new signalR.HubConnectionBuilder()
    .withUrl(`${this.configService.config.backendBaseUrl}/hub/lobby`, {
      withCredentials: true
    })
    .build();

  private readonly lobbyCreatedSubject = new Subject<Lobby>();
  private readonly lobbyClosedSubject = new Subject<string>();
  private readonly userJoinedLobbySubject = new Subject<string>();
  private readonly userLeftLobbySubject = new Subject<string>();
  private readonly userPlayedSoundSubject =
    new Subject<{ username: string; filePath: string }>();
  private readonly caseObtainedSubject = new Subject<number>();
  private readonly lobbyUserCountChangeSubject =
    new Subject<{ lobbyId: string; deltaCount: number }>();

  public readonly lobbyCreated$ = this.lobbyCreatedSubject.asObservable();
  public readonly lobbyClosed$ = this.lobbyClosedSubject.asObservable();
  public readonly userJoinedLobby$ = this.userJoinedLobbySubject.asObservable();
  public readonly userLeftLobby$ = this.userLeftLobbySubject.asObservable();
  public readonly userPlayedSound$ = this.userPlayedSoundSubject.asObservable();
  public readonly caseObtained$ = this.caseObtainedSubject.asObservable();
  public readonly lobbyUserCountChange$ = this.lobbyUserCountChangeSubject.asObservable();

  public async initializeConnection(): Promise<void> {
    if (this.initialized) {
      return;
    }

    try {
      if (this.initializing) {
        return;
      }
      try {
        this.initializing = true;
        await this.connection.start();
        this.addListeners();
        this.initialized = true;
      } finally {
        this.initializing = false;
      }
    } catch (error) {
      console.error(error);
    }
  }

  public async createLobby(name: string): Promise<Lobby | null> {
    const data = await this.connection.invoke<any>('CreateLobbyAsync', name,
      this.loginService.currentUser()?.id);
    if (data === null) {
      return null;
    }
    return lobbyZod.parse(data);
  }

  public async joinLobby(lobbyId: string): Promise<boolean> {
    return await this.connection.invoke<boolean>('JoinLobbyAsync', lobbyId, this.loginService.currentUser()?.id);
  }

  public async leaveLobby(lobbyId: string): Promise<boolean> {
    return await this.connection.invoke<boolean>('LeaveLobbyAsync');
  }

  public async getLobbies(): Promise<Lobby[]> {
    const data = await this.connection.invoke<any>('GetLobbiesAsync');
    return lobbyListZod.parse(data);
  }

  public async getLobbyById(lobbyId: string): Promise<Lobby | null> {
    const data = await this.connection.invoke<any>('GetLobbyByIdAsync', lobbyId);
    if (data === null) {
      return null;
    }
    return lobbyZod.parse(data);
  }

  public async getUsersInLobby(lobbyId: string): Promise<string[]> {
    const data = await this.connection.invoke<any>('GetUsersInLobbyAsync', lobbyId);
    return userListZod.parse(data);
  }

  public async playSound(soundId: number): Promise<boolean> {
    return await this.connection.invoke<boolean>('PlaySoundAsync', soundId);
  }

  private addListeners(): void {
    this.connection.on('ReceiveLobbyCreatedAsync', (data: any) => {
      const lobby: Lobby = lobbyZod.parse(data);
      this.lobbyCreatedSubject.next(lobby);
    });
    this.connection.on('ReceiveLobbyClosedAsync', (lobbyId: string) => {
      this.lobbyClosedSubject.next(lobbyId);
    });
    this.connection.on('ReceiveUserJoinedLobbyAsync', (username: string) => {
      this.userJoinedLobbySubject.next(username);
    });
    this.connection.on('ReceiveUserLeftLobbyAsync', (username: string) => {
      this.userLeftLobbySubject.next(username);
    });
    this.connection.on('ReceiveUserPlayedSoundAsync', (username: string, filePath: string) => {
      this.userPlayedSoundSubject.next({username, filePath});
    });
    this.connection.on('ReceiveCaseObtainedAsync', (caseId: number) => {
      this.caseObtainedSubject.next(caseId);
    });
    this.connection.on('ReceiveLobbyUserCountChangeAsync', (lobbyId: string, deltaCount: number) => {
      this.lobbyUserCountChangeSubject.next({lobbyId, deltaCount});
    });
  }
}

const lobbyZod = z.object({
  id: z.string().nonempty(),
  name: z.string().nonempty(),
  userCount: z.number().int().positive(),
});
export type Lobby = z.infer<typeof lobbyZod>;

const lobbyListZod = z.array(lobbyZod);

const userListZod = z.array(z.string().nonempty());
