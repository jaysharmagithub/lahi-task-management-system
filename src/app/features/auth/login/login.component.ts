import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    RouterModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatIconModule,
    MatCheckboxModule,
    MatProgressBarModule
  ],
  template: `
    <div class="auth-container">
      <mat-card class="auth-card">
        <mat-card-header>
          <mat-card-title>Login</mat-card-title>
          <mat-card-subtitle>Welcome back to Lahi Task Management</mat-card-subtitle>
        </mat-card-header>
        
        <mat-progress-bar *ngIf="isLoading()" mode="indeterminate"></mat-progress-bar>

        <mat-card-content>
          <form [formGroup]="loginForm" (ngSubmit)="onSubmit()">
            <mat-form-field appearance="outline" class="full-width">
              <mat-label>Email</mat-label>
              <input matInput formControlName="email" type="email" placeholder="email@example.com">
              <mat-error *ngIf="loginForm.get('email')?.hasError('required')">Email is required</mat-error>
              <mat-error *ngIf="loginForm.get('email')?.hasError('email')">Invalid email address</mat-error>
            </mat-form-field>

            <mat-form-field appearance="outline" class="full-width">
              <mat-label>Password</mat-label>
              <input matInput [type]="hidePassword() ? 'password' : 'text'" formControlName="password">
              <button mat-icon-button matSuffix (click)="hidePassword.set(!hidePassword())" type="button">
                <mat-icon>{{hidePassword() ? 'visibility_off' : 'visibility'}}</mat-icon>
              </button>
              <mat-error *ngIf="loginForm.get('password')?.hasError('required')">Password is required</mat-error>
            </mat-form-field>

            <div class="form-actions">
              <mat-checkbox formControlName="rememberMe">Remember Me</mat-checkbox>
            </div>

            <button mat-raised-button color="primary" class="full-width" type="submit" [disabled]="loginForm.invalid || isLoading()">
              Login
            </button>
          </form>
        </mat-card-content>

        <mat-card-footer>
          <div class="auth-footer">
            Don't have an account? <a routerLink="/auth/register">Register</a>
          </div>
        </mat-card-footer>
      </mat-card>
    </div>
  `,
  styles: [`
    .auth-container {
      display: flex;
      justify-content: center;
      align-items: center;
      height: 100vh;
      background-color: #f5f5f5;
    }
    .auth-card {
      width: 100%;
      max-width: 400px;
      padding: 20px;
    }
    .full-width {
      width: 100%;
      margin-bottom: 16px;
    }
    .form-actions {
      margin-bottom: 16px;
    }
    .auth-footer {
      padding: 16px;
      text-align: center;
    }
  `]
})
export class LoginComponent {
  private fb = inject(FormBuilder);
  private authService = inject(AuthService);
  private router = inject(Router);

  loginForm = this.fb.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required]],
    rememberMe: [false]
  });

  hidePassword = signal(true);
  isLoading = signal(false);

  onSubmit(): void {
    if (this.loginForm.valid) {
      this.isLoading.set(true);
      this.authService.login(this.loginForm.value as any).subscribe({
        next: () => {
          this.router.navigate(['/dashboard']);
        },
        error: () => {
          this.isLoading.set(false);
        }
      });
    }
  }
}
