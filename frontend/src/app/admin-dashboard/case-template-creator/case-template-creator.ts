import {Component, computed, inject, OnInit, Signal} from '@angular/core';
import {MatError, MatFormField, MatInput, MatLabel} from '@angular/material/input';
import { MatOption } from '@angular/material/core';
import { MatSelect } from '@angular/material/select';
import { Rarity, RaritySchema } from '../../../core/util/zod-schemas';
import {MatButton} from '@angular/material/button';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import {toSignal} from '@angular/core/rxjs-interop';
import {SnackbarService} from '../../../core/services/snackbar-service';

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
  protected snackbarService: SnackbarService = inject(SnackbarService);
  protected readonly Rarity = Rarity;
  protected readonly RaritySchema = RaritySchema;
  protected rarityOptions: {name: string, value: number}[] = [];
  private readonly formBuilder: FormBuilder = inject(FormBuilder);
  protected readonly formGroup = this.formBuilder.group({
    name: ['', [Validators.required, Validators.minLength(4)]],
    description: ['', [Validators.required, Validators.maxLength(200)]],
    rarity: ['', [Validators.required]],
  });

  private readonly formUpdatedSignal: Signal<void> = toSignal(this.formGroup.valueChanges);
  protected readonly formValid: Signal<boolean> = computed(() => {
    this.formUpdatedSignal();
    return this.formGroup.valid;
  });

  ngOnInit() {
    this.rarityOptions = Object.keys(Rarity)
      .filter(key => isNaN(Number(key)))
      .map(key => ({
        name: key,
        value: Rarity[key as keyof typeof Rarity]
      }));
  }

  public addCaseTemplate() {

    this.snackbarService.show("Case Template successfully added");
  }
}
