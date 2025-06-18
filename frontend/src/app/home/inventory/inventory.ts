import { Component, OnInit, signal, computed, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatInputModule } from '@angular/material/input';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { FormsModule } from '@angular/forms';
import { MatTabsModule } from '@angular/material/tabs';
import { Sound, SoundService } from '../../../core/services/sound-service';
import { Case, CaseService } from '../../../core/services/case-service';
import { MatButtonModule } from '@angular/material/button';
import { MatTooltipModule } from '@angular/material/tooltip';
import { ConfigService } from '../../../core/services/config-service';
import { MatSnackBar } from '@angular/material/snack-bar';
import {CaseOpeningStateService} from '../util/case-opening-state-service';

@Component({
  selector: 'app-inventory',
  standalone: true,
  imports: [
    CommonModule,
    MatCardModule,
    MatInputModule,
    MatFormFieldModule,
    FormsModule,
    MatTabsModule,
    MatIconModule,
    MatButtonModule,
    MatTooltipModule
  ],
  templateUrl: './inventory.html',
  styleUrls: ['./inventory.scss']
})
export class InventoryComponent implements OnInit {
  private readonly configService = inject(ConfigService);
  private readonly snackBar = inject(MatSnackBar);
  private currentAudio: HTMLAudioElement | null = null;

  protected sounds = signal<Sound[]>([]);
  protected cases = signal<Case[]>([]);
  protected filterValue = signal('');
  protected currentlyPlaying = signal<number | null>(null);
  protected selectedTab = signal(0);
  protected isOpeningCase = signal(false);

  protected filteredSounds = computed(() => {
    const filter = this.filterValue().toLowerCase();
    return this.sounds().filter(sound =>
      sound.name.toLowerCase().includes(filter) ||
      sound.description.toLowerCase().includes(filter) ||
      sound.rarity.toLowerCase().includes(filter)
    );
  });

  protected filteredCases = computed(() => {
    const filter = this.filterValue().toLowerCase();
    return this.cases().filter(case_ =>
      case_.name.toLowerCase().includes(filter) ||
      case_.description.toLowerCase().includes(filter) ||
      case_.rarity.toLowerCase().includes(filter)
    );
  });

  constructor(
    private soundService: SoundService,
    private caseService: CaseService,
    private caseOpeningState: CaseOpeningStateService
  ) {}

  async ngOnInit() {
    await Promise.all([
      this.loadSounds(),
      this.loadCases()
    ]);
  }

  private async loadSounds(): Promise<void> {
    this.sounds.set(await this.soundService.getAllSoundsOfUser());
  }

  private async loadCases(): Promise<void> {
    this.cases.set(await this.caseService.getAllCasesOfUser());
  }

  applyFilter(event: Event) {
    const value = (event.target as HTMLInputElement).value;
    this.filterValue.set(value.trim().toLowerCase());
  }

  async toggleSound(sound: Sound): Promise<void> {
    if (this.currentlyPlaying() === sound.id) {
      this.currentAudio?.pause();
      this.currentAudio = null;
      this.currentlyPlaying.set(null);
      return;
    }

    if (this.currentAudio) {
      this.currentAudio.pause();
      this.currentAudio = null;
      this.currentlyPlaying.set(null);
    }

    try {
      this.currentlyPlaying.set(sound.id);
      this.currentAudio = new Audio(`${this.configService.config.soundsBaseUrl}/${sound.filePath}`);

      this.currentAudio.onended = () => {
        this.currentAudio = null;
        this.currentlyPlaying.set(null);
      };

      await this.currentAudio.play();
    } catch (error) {
      console.error('Error playing sound:', error);
      this.currentAudio = null;
      this.currentlyPlaying.set(null);
    }
  }

  async openCase(case_: Case): Promise<void> {
    if (this.isOpeningCase()) return;

    try {
      this.isOpeningCase.set(true);
      this.caseOpeningState.startOpening(case_);

      const sound = await this.caseService.openCase(case_.id);

      if (sound) {
        // Update the state with the obtained sound
        this.caseOpeningState.setObtainedSound(sound);

        // Add the new sound to the inventory after a delay
        setTimeout(() => {
          this.sounds.update(sounds => [...sounds, sound]);
          this.caseOpeningState.reset();
        }, 3000); // Show the reward for 3 seconds
      } else {
        this.caseOpeningState.reset();
        this.snackBar.open('Failed to open case', 'Close', { duration: 3000 });
      }
    } catch (error) {
      console.error('Error opening case:', error);
      this.caseOpeningState.reset();
      this.snackBar.open('Error opening case', 'Close', { duration: 3000 });
    } finally {
      this.isOpeningCase.set(false);
    }
  }
}
