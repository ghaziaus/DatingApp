import { Component, OnInit } from '@angular/core';
import { AuthService } from '../_services/auth.service';

@Component({
  selector: 'app-nav',
  templateUrl: './nav.component.html',
  styleUrls: ['./nav.component.scss']
})
export class NavComponent implements OnInit {

  model: any = {};

  constructor( private authServicee: AuthService) { }

  ngOnInit() {
  }

  login(): void{
    this.authServicee.login(this.model).subscribe(next => {
      console.log('logged in successfully');
    }, error => {
      console.log('Failed to login');
    });
  }

  loggedIn(): boolean{
    const token = localStorage.getItem('token');
    return !!token;
  }

  logout(): void{
    localStorage.removeItem('token');
    console.log('logged out');
  }
}
