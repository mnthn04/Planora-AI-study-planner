import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterModule, ActivatedRoute } from '@angular/router';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule],
  template: `
    <div class="auth-container">
      <div class="glass-card auth-card">
        <h2 class="auth-title">Welcome Back</h2>
        <p class="auth-subtitle">Log in to your study dashboard</p>
        
        <form [formGroup]="loginForm" (ngSubmit)="onSubmit()">
          <div class="form-group">
            <label>Email</label>
            <input type="email" formControlName="email" placeholder="john@example.com">
          </div>
          
          <div class="form-group">
            <label>Password</label>
            <input type="password" formControlName="password" placeholder="••••••••">
          </div>
          
          @if (error) {
            <p class="error-msg">{{ error }}</p>
          }
          
          <button type="submit" class="btn-primary auth-btn" [disabled]="loginForm.invalid || loading">
            {{ loading ? 'Logging in...' : 'Login' }}
          </button>
        </form>
        
        <p class="auth-footer">
          Don't have an account? <a routerLink="/register">Register</a>
        </p>
      </div>
    </div>
  `,
  styles: [`
    .auth-container {
      display: flex;
      justify-content: center;
      align-items: center;
      min-height: calc(100vh - 100px);
    }
    .auth-card {
      width: 100%;
      max-width: 400px;
    }
    .auth-title {
      text-align: center;
      margin-bottom: 8px;
    }
    .auth-subtitle {
      text-align: center;
      color: var(--text-muted);
      margin-bottom: 32px;
    }
    .auth-btn {
      width: 100%;
      margin-top: 10px;
    }
    .auth-footer {
      text-align: center;
      margin-top: 24px;
      font-size: 0.9rem;
      color: var(--text-muted);
    }
    .auth-footer a {
      color: var(--primary);
      text-decoration: none;
      font-weight: 600;
    }
    .error-msg {
      color: #ef4444;
      font-size: 0.875rem;
      margin-bottom: 16px;
    }
  `]
})
export class LoginComponent {
  private fb = inject(FormBuilder);
  private authService = inject(AuthService);
  private router = inject(Router);
  private route = inject(ActivatedRoute);

  loginForm = this.fb.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', Validators.required]
  });

  loading = false;
  error = '';

  onSubmit() {
    if (this.loginForm.valid) {
      this.loading = true;
      this.error = '';
      this.authService.login(this.loginForm.value).subscribe({
        next: () => {
          const returnUrl = this.route.snapshot.queryParams['returnUrl'] || '/dashboard';
          this.router.navigateByUrl(returnUrl);
        },
        error: (err) => {
          this.error = 'Invalid email or password.';
          this.loading = false;
        }
      });
    }
  }
}
