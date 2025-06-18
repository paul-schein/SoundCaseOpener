import { Component } from '@angular/core';
import {FileUpload} from './file-upload/file-upload';
import {CaseTemplateCreator} from "./case-template-creator/case-template-creator";
import {SoundTemplateCreator} from './sound-template-creator/sound-template-creator';

@Component({
  selector: 'app-admin-dashboard',
  imports: [
    FileUpload,
    CaseTemplateCreator,
    SoundTemplateCreator
  ],
  templateUrl: './admin-dashboard.html',
  styleUrl: './admin-dashboard.scss'
})
export class AdminDashboard {

}
