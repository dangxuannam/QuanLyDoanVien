import { Component } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { MatSnackBar } from '@angular/material/snack-bar';
import { ApiService } from '../../core/services/api.service';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html'
})
export class LoginComponent {
  form: FormGroup;
  loading = false;
  hidePassword = true;

  constructor(
    private fb: FormBuilder,
    private apiService: ApiService,
    private authService: AuthService,
    private router: Router,
    private snackBar: MatSnackBar
  ) {
    this.form = this.fb.group({
      username: ['', Validators.required],
      password: ['', Validators.required]
    });
    if (this.authService.isLoggedIn()) {
      this.router.navigate(['/']);
    }
  }

  login(): void {
    if (this.form.invalid) return;
    this.loading = true;
    this.apiService.login(this.form.value).subscribe({
      next: (res) => {
        this.authService.saveSession(res);
        this.router.navigate(['/dashboard']);
      },
      error: (err) => {
        const msg = err.error?.message || 'Đăng nhập thất bại. Kiểm tra lại thông tin.';
        this.snackBar.open(msg, 'Đóng', { duration: 4000, panelClass: 'snack-error' });
        this.loading = false;
      },
      complete: () => { this.loading = false; }
    });
  }
}
