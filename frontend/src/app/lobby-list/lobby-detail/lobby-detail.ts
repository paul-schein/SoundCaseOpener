import {Component, inject, input, InputSignal, InputSignalWithTransform, OnInit} from '@angular/core';
import {LobbyService} from '../../../core/lobby.service';

@Component({
  selector: 'app-lobby-detail',
  imports: [],
  templateUrl: './lobby-detail.html',
  styleUrl: './lobby-detail.scss'
})
export class LobbyDetail implements OnInit {
  public lobbyId: InputSignal<string> = input.required<string>();
  public isCreator: InputSignalWithTransform<boolean, string> = input(false, {
    transform: (value: string) => value === 'true'
  });

  private readonly lobbyService = inject(LobbyService);

  public async ngOnInit(): Promise<void> {
    console.log(`LobbyDetail initialized with lobbyId: ${this.lobbyId()}`);
    console.log(`Is creator: ${this.isCreator()}`);
  }
}
