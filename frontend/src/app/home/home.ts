import { Component } from '@angular/core';
import {Navbar} from '../navbar/navbar';
import {RouterOutlet} from '@angular/router';
import {InventoryComponent} from './inventory/inventory';

@Component({
  selector: 'app-home',
  imports: [
    InventoryComponent
  ],
  templateUrl: './home.html',
  styleUrl: './home.scss'
})
export class Home {

}
