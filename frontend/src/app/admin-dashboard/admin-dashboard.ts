import { Component } from '@angular/core';
import {FileUpload} from './file-upload/file-upload';
import {CaseTemplateCreator} from "./case-template-creator/case-template-creator";
import {SoundTemplateCreator} from './sound-template-creator/sound-template-creator';
import {MatTab, MatTabGroup} from '@angular/material/tabs';
import {MatCard, MatCardContent, MatCardHeader, MatCardTitle} from '@angular/material/card';
import {CaseTemplateEditor} from './case-template-editor/case-template-editor';

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
    CaseTemplateEditor
  ],
  templateUrl: './admin-dashboard.html',
  styleUrl: './admin-dashboard.scss'
})
export class AdminDashboard {

}
