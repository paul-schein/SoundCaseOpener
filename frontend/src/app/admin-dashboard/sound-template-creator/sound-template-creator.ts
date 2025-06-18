import {Component, computed, inject, OnInit, signal, Signal, WritableSignal} from '@angular/core';
import {MatError, MatFormField, MatInput, MatLabel} from '@angular/material/input';
import { MatOption } from '@angular/material/core';
import { MatSelect } from '@angular/material/select';
import { Rarity, RaritySchema } from '../../../core/util/zod-schemas';
import {MatButton} from '@angular/material/button';
import {FormBuilder, ReactiveFormsModule, Validators} from '@angular/forms';
import {toSignal} from '@angular/core/rxjs-interop';
import {SnackbarService} from '../../../core/services/snackbar-service';
import {ConfigService} from '../../../core/services/config-service';
import {NewSoundTemplate, SoundTemplateService} from '../../../core/services/sound-template-service';
import {SoundFile, SoundFileService} from '../../../core/services/sound-file-service';

@Component({
  selector: 'app-sound-template-creator',
  standalone: true,
    imports: [
      MatFormField,
      MatLabel,
      MatInput,
      MatOption,
      MatSelect,
      MatButton,
      MatError,
      ReactiveFormsModule
    ],
  templateUrl: './sound-template-creator.html',
  styleUrl: './sound-template-creator.scss'
})
export class SoundTemplateCreator implements OnInit {
  protected configService: ConfigService = inject(ConfigService);
  protected snackbarService: SnackbarService = inject(SnackbarService);
  protected soundService: SoundTemplateService = inject(SoundTemplateService);
  protected soundFileService: SoundFileService = inject(SoundFileService);
  protected soundFileList: WritableSignal<SoundFile[]> = signal([]);
  protected rarityOptions: {name: string, value: string}[] = [];
  private readonly formBuilder: FormBuilder = inject(FormBuilder);
  protected readonly formGroup = this.formBuilder.group({
    name: ['', [Validators.required, Validators.minLength(this.configService.config.nameMinLength)]],
    description: ['', [Validators.required, Validators.maxLength(this.configService.config.descriptionMaxLength)]],
    rarity: ['', [Validators.required]],
    minCooldown: [0, [Validators.required, Validators.pattern(/^[0-9]+(\.[0-9]+)?$/)]],
    maxCooldown: [0, [Validators.required, Validators.pattern(/^[0-9]+(\.[0-9]+)?$/)]],
    soundFile: [0, [Validators.required]],
  });

  private readonly formUpdatedSignal: Signal<void> = toSignal(this.formGroup.valueChanges);
  protected readonly formValid: Signal<boolean> = computed(() => {
    this.formUpdatedSignal();
    return this.formGroup.valid;
  });

  protected readonly minCooldownValue = computed(() => {
    this.formUpdatedSignal();
    return Number(this.formGroup.get('minCooldown')?.value || 0);
  });

  protected readonly maxCooldownValue = computed(() => {
    this.formUpdatedSignal();
    return Number(this.formGroup.get('maxCooldown')?.value || 0);
  });

  protected readonly isCooldownValid = computed(() => {
    return this.maxCooldownValue() >= this.minCooldownValue();
  });

  public async ngOnInit() {
    this.rarityOptions = Object.values(RaritySchema.enum).map(value => ({
      name: value,
      value: value
    }));
    const result = await this.soundFileService.getAllFiles();
    if (result) {
      this.soundFileList.set(result.soundFiles);
    }
  }

  public async addSoundTemplate() {
    if (!this.formGroup.valid) {
      this.snackbarService.show("Please fill all required fields correctly");
      return;
    }

    const formValues = this.formGroup.value;
    if (!this.isCooldownValid()) {
      this.snackbarService.show("Maximum cooldown can't be smaller than the minimum cooldown");
      return;
    }

    const newSoundTemplate: NewSoundTemplate = {
      name: formValues.name as string,
      description: formValues.description as string,
      rarity: formValues.rarity as Rarity,
      minCooldown: formValues.minCooldown as number,
      maxCooldown: formValues.minCooldown as number,
      soundFileId: formValues.soundFile as number
    };

    const result = await this.soundService.addSoundTemplate(newSoundTemplate);

    if (result) {
      this.snackbarService.show("Sound Template successfully added");
      this.formGroup.reset();
    } else {
      this.snackbarService.show("There was an error trying to add a Sound Template");
    }
  }
}
