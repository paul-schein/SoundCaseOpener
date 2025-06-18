import { Component } from '@angular/core';
import {Navbar} from '../navbar/navbar';
import {RouterOutlet} from '@angular/router';
import {InventoryComponent} from './inventory/inventory';
import {CaseOpeningComponent} from './case-opening/case-opening';

@Component({
  selector: 'app-home',
  imports: [
    InventoryComponent,
    CaseOpeningComponent
  ],
  templateUrl: './home.html',
  styleUrl: './home.scss'
})
export class Home {

}
