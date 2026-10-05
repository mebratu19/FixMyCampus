import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';

import { AuthService } from '../../services/auth';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './login.html',
  styleUrl: './login.scss'
})
export class Login {
  private authService = inject(AuthService);
  private router = inject(Router);

  email = '';
  password = '';

  loading = false;
  errorMessage = '';

  login(): void {
    this.errorMessage = '';

    if (!this.email || !this.password) {
      this.errorMessage = 'Please enter your email and password.';
      return;
    }

    this.loading = true;

    this.authService.login({
      email: this.email,
      password: this.password
    }).subscribe({
     next: (response) => {
  this.authService.saveSession(response);

  if (response.user.role === 'Admin') {
    this.router.navigate(['/admin']);
  } else {
    this.router.navigate(['/']);
  }
},
      error: (error) => {
        this.loading = false;

        this.errorMessage =
          error?.error?.message ??
          'Login failed. Please check your email and password.';
      },
      complete: () => {
        this.loading = false;
      }
    });
  }
}