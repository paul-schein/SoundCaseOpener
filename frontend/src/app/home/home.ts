import {Component, inject, OnInit} from '@angular/core';
import {LobbyService} from '../../core/lobby.service';

@Component({
  selector: 'app-home',
  imports: [],
  templateUrl: './home.html',
  styleUrl: './home.scss'
})
export class Home implements OnInit {
  private readonly lobbyService: LobbyService = inject(LobbyService);

  public async ngOnInit(): Promise<void> {
    await this.lobbyService.initializeConnection();
    await this.lobbyService.createLobby('Test Lobby');
  }
}
