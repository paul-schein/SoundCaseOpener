import { Component } from '@angular/core';
import {FileUpload} from './file-upload/file-upload';

@Component({
  selector: 'app-admin-dashboard',
  imports: [
    FileUpload
  ],
  templateUrl: './admin-dashboard.html',
  styleUrl: './admin-dashboard.scss'
})
export class AdminDashboard {

}
