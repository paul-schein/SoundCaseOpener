import {Component, computed, inject, OnInit, signal, Signal, WritableSignal} from '@angular/core';
import {MatError, MatFormField, MatInput, MatLabel} from '@angular/material/input';
import { MatOption } from '@angular/material/core';
import { MatSelect } from '@angular/material/select';
import { Rarity, RaritySchema } from '../../../core/util/zod-schemas';
import {MatButton} from '@angular/material/button';
import {FormBuilder, FormGroup, ReactiveFormsModule, Validators} from '@angular/forms';
import {toSignal} from '@angular/core/rxjs-interop';
import {SnackbarService} from '../../../core/services/snackbar-service';
import {ConfigService} from '../../../core/config.service';
import {
  NewSoundTemplate,
  SoundTemplateResponse,
  SoundTemplateService
} from '../../../core/services/sound-template-service';
import {SoundFile, SoundFileService} from '../../../core/services/sound-file-service';
import {
  CaseTemplate,
  CaseTemplateService,
  NewItemTemplateToCaseTemplate
} from '../../../core/services/case-template-service';

@Component({
  selector: 'app-case-template-editor',
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
  templateUrl: './case-template-editor.html',
  styleUrl: './case-template-editor.scss'
})
export class CaseTemplateEditor implements OnInit {
  protected snackbarService: SnackbarService = inject(SnackbarService);
  protected caseTemplateService: CaseTemplateService = inject(CaseTemplateService);
  protected soundTemplateService: SoundTemplateService = inject(SoundTemplateService);
  protected caseTemplateList: WritableSignal<CaseTemplate[]> = signal([]);
  protected soundTemplateList: WritableSignal<SoundTemplateResponse[]> = signal([]);
  private readonly formBuilder: FormBuilder = inject(FormBuilder);
  protected readonly formGroup = this.formBuilder.group({
    caseTemplate: [0, [Validators.required]],
    itemTemplate: [0, [Validators.required]],
    weight: [1, [Validators.required, Validators.pattern(/^(?:0|[1-9]\d*)(?:\.\d+)?$/)]],
  });

  private readonly formUpdatedSignal: Signal<void> = toSignal(this.formGroup.valueChanges);
  protected readonly formValid: Signal<boolean> = computed(() => {
    this.formUpdatedSignal();
    return this.formGroup.valid;
  });

  public async ngOnInit() {
    const soundTemplatesResult = await this.soundTemplateService.getAllSoundTemplates();
    if (soundTemplatesResult) {
      this.soundTemplateList.set(soundTemplatesResult.soundTemplates);
    }

    const caseTemplatesResult = await this.caseTemplateService.getAllCaseTemplates();
    if (caseTemplatesResult) {
      this.caseTemplateList.set(caseTemplatesResult.caseTemplates);
    }
  }

  public async addToCaseTemplate() {
    if (!this.formGroup.valid) {
      this.snackbarService.show("Please fill all required fields correctly");
      return;
    }

    const formValues = this.formGroup.value;

    const newItemTemplateToCaseTemplate: NewItemTemplateToCaseTemplate = {
      caseTemplateId: formValues.caseTemplate as number,
      itemTemplateId: formValues.itemTemplate as number,
      weight: formValues.weight as number,
    };

    const result =
      await this.caseTemplateService.addToCaseTemplate(newItemTemplateToCaseTemplate);

    if (result) {
      this.snackbarService.show("Sound Template successfully added to Case Template");
      this.formGroup.reset();
    } else {
      this.snackbarService.show("There was an error trying to add a Sound Template to a Case Template");
    }
  }
}
