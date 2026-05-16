import { Component, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './login.html',
  styleUrls: ['./login.scss'],
})
export class Login {
  userName = '';
  password = '';
  errorMessage = '';
  successMessage = '';
  private returnUrl = '/';
  private readonly passwordPattern = /^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{6,100}$/;

  constructor(
    private authService: AuthService,
    private router: Router,
    private route: ActivatedRoute,
    private cdr: ChangeDetectorRef,
  ) {
    this.returnUrl = this.route.snapshot.queryParamMap.get('returnUrl') || '/';
  }

  onInputChange() {
    this.errorMessage = '';
    this.successMessage = '';
    this.cdr.detectChanges();
  }

  login() {
    this.errorMessage = '';
    this.successMessage = '';
    this.cdr.detectChanges();

    const normalizedUserName = this.userName.trim();

    if (!normalizedUserName || !this.password) {
      this.errorMessage = 'Username and password are required.';
      this.cdr.detectChanges();
      return;
    }

    if (normalizedUserName.length < 3) {
      this.errorMessage = 'Username must be at least 3 characters long.';
      this.cdr.detectChanges();
      return;
    }

    if (!this.passwordPattern.test(this.password)) {
      this.errorMessage = 'Password must be at least 6 characters and include an uppercase letter, a lowercase letter, and a number.';
      this.cdr.detectChanges();
      return;
    }

    this.authService
      .login({
        userName: normalizedUserName,
        password: this.password,
      })
      .subscribe({
        next: () => {
          this.successMessage = 'Login successful! Redirecting...';
          this.cdr.detectChanges();

          setTimeout(() => this.router.navigateByUrl(this.returnUrl), 1000);
        },
        error: (err) => {
          console.log('LOGIN ERROR:', err);

          if (err.error?.errors) {
            const allErrors = Object.values(err.error.errors).flat();
            this.errorMessage = allErrors.join(' ');
            this.cdr.detectChanges();
            return;
          }

          if (err.error?.message) {
            this.errorMessage = err.error.message;
            this.cdr.detectChanges();
            return;
          }

          if (typeof err.error === 'string') {
            this.errorMessage = err.error;
            this.cdr.detectChanges();
            return;
          }

          if (err.message) {
            this.errorMessage = err.message;
            this.cdr.detectChanges();
            return;
          }

          this.errorMessage = 'Login failed.';
          this.cdr.detectChanges();
        },
      });
  }
}
