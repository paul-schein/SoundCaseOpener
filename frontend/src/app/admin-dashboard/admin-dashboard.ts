import {Component, inject, signal, WritableSignal} from '@angular/core';
import {FileUpload} from './file-upload/file-upload';
import {CaseTemplateCreator} from "./case-template-creator/case-template-creator";
import {SoundTemplateCreator} from './sound-template-creator/sound-template-creator';
import {MatTab, MatTabGroup} from '@angular/material/tabs';
import {MatCard, MatCardContent, MatCardHeader, MatCardTitle} from '@angular/material/card';
import {CaseTemplateEditor} from './case-template-editor/case-template-editor';
import {SoundTemplateDeletor} from './sound-template-deletor/sound-template-deletor';
import {SnackbarService} from '../../core/services/snackbar-service';
import {CaseTemplate, CaseTemplateService} from '../../core/services/case-template-service';
import {SoundTemplateResponse, SoundTemplateService} from '../../core/services/sound-template-service';
import {SoundFileService} from '../../core/services/sound-file-service';
import {ConfigService} from '../../core/services/config-service';

@Component({
  selector: 'app-admin-dashboard',
  imports: [
    FileUpload,
    CaseTemplateCreator,
    SoundTemplateCreator,
    MatTabGroup,
    MatTab,
    MatCard,
    MatCardHeader,
    MatCardTitle,
    MatCardContent,
    CaseTemplateEditor,
    SoundTemplateDeletor
  ],
  templateUrl: './admin-dashboard.html',
  styleUrl: './admin-dashboard.scss'
})
export class AdminDashboard {
  protected snackbarService: SnackbarService = inject(SnackbarService);
  protected configService: ConfigService = inject(ConfigService);
  protected caseTemplateService: CaseTemplateService = inject(CaseTemplateService);
  protected soundTemplateService: SoundTemplateService = inject(SoundTemplateService);
  protected caseTemplateList: WritableSignal<CaseTemplate[]> = signal([]);
  protected soundTemplateList: WritableSignal<SoundTemplateResponse[]> = signal([]);
  protected soundFileService: SoundFileService = inject(SoundFileService);
  protected files: WritableSignal<File[]> = signal([]);
}
