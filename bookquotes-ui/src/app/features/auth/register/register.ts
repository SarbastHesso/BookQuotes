import { Component, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './register.html',
  styleUrls: ['./register.scss'],
})
export class Register {
  userName = '';
  password = '';
  confirmPassword = '';
  errorMessage = '';
  successMessage = '';
  private readonly passwordPattern = /^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{6,100}$/;

  constructor(
    private authService: AuthService,
    private router: Router,
    private cdr: ChangeDetectorRef,
  ) {}

  onInputChange() {
    this.errorMessage = '';
    this.successMessage = '';
    this.cdr.detectChanges();
  }

  register() {
    this.errorMessage = '';
    this.successMessage = '';
    this.cdr.detectChanges();

    const normalizedUserName = this.userName.trim();

    if (!normalizedUserName || !this.password || !this.confirmPassword) {
      this.errorMessage = 'All fields are required.';
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

    if (this.password !== this.confirmPassword) {
      this.errorMessage = 'Passwords do not match.';
      this.cdr.detectChanges();
      return;
    }

    this.authService
      .register({
        userName: normalizedUserName,
        password: this.password,
      })
      .subscribe({
        next: () => {
          this.successMessage = 'Registration successful! Redirecting to login...';
          this.cdr.detectChanges();
          setTimeout(() => this.router.navigate(['/login']), 1500);
        },
        error: (err) => {
          console.log('REGISTER ERROR:', err);

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

          this.errorMessage = 'Registration failed.';
          this.cdr.detectChanges();
        },
      });
  }
}
