import {Component, computed, inject, OnInit, Signal} from '@angular/core';
import {MatError, MatFormField, MatInput, MatLabel} from '@angular/material/input';
import { MatOption } from '@angular/material/core';
import { MatSelect } from '@angular/material/select';
import { Rarity, RaritySchema } from '../../../core/util/zod-schemas';
import {MatButton} from '@angular/material/button';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import {toSignal} from '@angular/core/rxjs-interop';
import {SnackbarService} from '../../../core/services/snackbar-service';
import {ConfigService} from '../../../core/config.service';
import {CaseTemplateService, NewCaseTemplate} from '../../../core/services/case-template-service';

@Component({
  selector: 'app-case-template-creator',
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
  templateUrl: './case-template-creator.html',
  styleUrl: './case-template-creator.scss'
})
export class CaseTemplateCreator implements OnInit {
  protected configService: ConfigService = inject(ConfigService);
  protected snackbarService: SnackbarService = inject(SnackbarService);
  protected caseTemplateService: CaseTemplateService = inject(CaseTemplateService);
  protected readonly RaritySchema = RaritySchema;
  protected rarityOptions: {name: string, value: string}[] = [];
  private readonly formBuilder: FormBuilder = inject(FormBuilder);
  protected readonly formGroup = this.formBuilder.group({
    name: ['', [Validators.required, Validators.minLength(this.configService.config.nameMinLength)]],
    description: ['', [Validators.required, Validators.maxLength(this.configService.config.descriptionMaxLength)]],
    rarity: ['', [Validators.required]],
  });

  private readonly formUpdatedSignal: Signal<void> = toSignal(this.formGroup.valueChanges);
  protected readonly formValid: Signal<boolean> = computed(() => {
    this.formUpdatedSignal();
    return this.formGroup.valid;
  });

  ngOnInit() {
    this.rarityOptions = Object.values(RaritySchema.enum).map(value => ({
      name: value,
      value: value
    }));
  }

  public async addCaseTemplate() {
    if (!this.formGroup.valid) {
      this.snackbarService.show("Please fill all required fields correctly");
      return;
    }

    const formValues = this.formGroup.value;

    const newCaseTemplate: NewCaseTemplate = {
      name: formValues.name as string,
      description: formValues.description as string,
      rarity: formValues.rarity as unknown as Rarity
    };

    const result = await this.caseTemplateService.addCaseTemplate(newCaseTemplate);

    if (result) {
      this.snackbarService.show("Case Template successfully added");
      this.formGroup.reset();
    } else {
      this.snackbarService.show("There was an error trying to add a Case Template");
    }  }
}
