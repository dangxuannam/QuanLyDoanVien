import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from './core/services/auth.service';

@Component({
  selector: 'app-root',
  template: '<router-outlet></router-outlet>'
})
export class AppComponent {
  constructor(private authService: AuthService, private router: Router) {
    if (!authService.isLoggedIn()) {
      router.navigate(['/login']);
    }
  }
}
