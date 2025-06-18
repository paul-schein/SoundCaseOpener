import { Component } from '@angular/core';
import {FileUpload} from './file-upload/file-upload';
import {CaseTemplateCreator} from "./case-template-creator/case-template-creator";

@Component({
  selector: 'app-admin-dashboard',
    imports: [
        FileUpload,
        CaseTemplateCreator
    ],
  templateUrl: './admin-dashboard.html',
  styleUrl: './admin-dashboard.scss'
})
export class AdminDashboard {

}
