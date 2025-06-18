import { Component } from '@angular/core';
import {FileUpload} from './file-upload/file-upload';
import {CaseTemplateCreator} from "./case-template-creator/case-template-creator";
import {SoundTemplateCreator} from './sound-template-creator/sound-template-creator';
import {CaseTemplateEditor} from './case-template-editor/case-template-editor';
import {MatTab, MatTabGroup} from '@angular/material/tabs';
import {MatCard, MatCardContent, MatCardHeader, MatCardTitle} from '@angular/material/card';

@Component({
  selector: 'app-admin-dashboard',
  imports: [
    FileUpload,
    CaseTemplateCreator,
    SoundTemplateCreator,
    CaseTemplateEditor,
    MatTabGroup,
    MatTab,
    MatCard,
    MatCardHeader,
    MatCardTitle,
    MatCardContent
  ],
  templateUrl: './admin-dashboard.html',
  styleUrl: './admin-dashboard.scss'
})
export class AdminDashboard {

}
