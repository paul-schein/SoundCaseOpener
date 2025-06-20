import {Component, computed, inject, input, InputSignal, Signal, signal, WritableSignal} from '@angular/core';
import {FormBuilder, FormsModule, ReactiveFormsModule, Validators} from '@angular/forms';
import {SoundTemplateResponse, SoundTemplateService} from '../../../core/services/sound-template-service';
import {MatError, MatFormField, MatLabel} from '@angular/material/input';
import {MatOption} from '@angular/material/core';
import {MatSelect} from '@angular/material/select';
import {toSignal} from '@angular/core/rxjs-interop';
import {SnackbarService} from '../../../core/services/snackbar-service';
import {MatButton} from '@angular/material/button';

@Component({
  selector: 'app-sound-template-deletor',
  imports: [
    FormsModule,
    MatError,
    MatFormField,
    MatLabel,
    MatOption,
    MatSelect,
    ReactiveFormsModule,
    MatFormField,
    MatLabel,
    MatSelect,
    MatButton
  ],
  templateUrl: './sound-template-deletor.html',
  styleUrl: './sound-template-deletor.scss'
})
export class SoundTemplateDeletor {
  protected soundTemplateList: WritableSignal<SoundTemplateResponse[]> = signal([]);
  public snackbarService: InputSignal<SnackbarService> = input.required();
  public soundTemplateService: InputSignal<SoundTemplateService> = input.required();
  private readonly formBuilder: FormBuilder = inject(FormBuilder);
  protected readonly formGroup = this.formBuilder.group({
    itemTemplate: [0, [Validators.required]],
  });

  private readonly formUpdatedSignal: Signal<void> = toSignal(this.formGroup.valueChanges);
  protected readonly formValid: Signal<boolean> = computed(() => {
    this.formUpdatedSignal();
    return this.formGroup.valid;
  });

  public async ngOnInit() {
    const soundTemplatesResult = await this.soundTemplateService().getAllSoundTemplates();
    if (soundTemplatesResult) {
      this.soundTemplateList.set(soundTemplatesResult.soundTemplates);
    }
  }

  public async deleteTemplate() {
    if (!this.formGroup.valid) {
      this.snackbarService().show("Please fill all required fields correctly");
      return;
    }

    const formValues = this.formGroup.value;
    const result =
      await this.soundTemplateService().deleteSoundTemplate(formValues.itemTemplate as number);

    if (result) {
      this.snackbarService().show("Sound Template successfully deleted");
      this.formGroup.reset();
    } else {
      this.snackbarService().show("There was an error trying to delete the Sound Template");
    }
  }
}
