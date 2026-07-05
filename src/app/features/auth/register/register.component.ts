import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatSelectModule } from '@angular/material/select';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { AuthService } from '../../../core/services/auth.service';
import { CustomValidators } from '../../../core/validators/custom-validators';
import { UserRole } from '../../../core/models/auth.model';

@Component({
  selector: 'app-register',
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
    MatSelectModule,
    MatProgressBarModule
  ],
  template: `
    <div class="auth-container">
      <mat-card class="auth-card">
        <mat-card-header>
          <mat-card-title>Register</mat-card-title>
          <mat-card-subtitle>Create your account</mat-card-subtitle>
        </mat-card-header>

        <mat-progress-bar *ngIf="isLoading()" mode="indeterminate"></mat-progress-bar>

        <mat-card-content>
          <form [formGroup]="registerForm" (ngSubmit)="onSubmit()">
            <mat-form-field appearance="outline" class="full-width">
              <mat-label>Full Name</mat-label>
              <input matInput formControlName="fullName" placeholder="John Doe">
              <mat-error *ngIf="registerForm.get('fullName')?.hasError('required')">Name is required</mat-error>
            </mat-form-field>

            <mat-form-field appearance="outline" class="full-width">
              <mat-label>Email</mat-label>
              <input matInput formControlName="email" type="email" placeholder="email@example.com">
              <mat-error *ngIf="registerForm.get('email')?.hasError('required')">Email is required</mat-error>
              <mat-error *ngIf="registerForm.get('email')?.hasError('email')">Invalid email address</mat-error>
            </mat-form-field>

            <div class="row">
              <mat-form-field appearance="outline" class="col">
                <mat-label>Role</mat-label>
                <mat-select formControlName="role">
                  <mat-option [value]="roles.Admin">Admin</mat-option>
                  <mat-option [value]="roles.Employee">Employee</mat-option>
                </mat-select>
              </mat-form-field>
            </div>

            <div class="row">
              <mat-form-field appearance="outline" class="col">
                <mat-label>Department</mat-label>
                <input matInput formControlName="department">
              </mat-form-field>
              <mat-form-field appearance="outline" class="col">
                <mat-label>Designation</mat-label>
                <input matInput formControlName="designation">
              </mat-form-field>
            </div>

            <mat-form-field appearance="outline" class="full-width">
              <mat-label>Password</mat-label>
              <input matInput [type]="hidePassword() ? 'password' : 'text'" formControlName="password">
              <button mat-icon-button matSuffix (click)="hidePassword.set(!hidePassword())" type="button">
                <mat-icon>{{hidePassword() ? 'visibility_off' : 'visibility'}}</mat-icon>
              </button>
              <mat-error *ngIf="registerForm.get('password')?.hasError('required')">Password is required</mat-error>
              <mat-error *ngIf="registerForm.get('password')?.hasError('passwordComplexity')">
                Must be 8+ chars with uppercase, lowercase and number
              </mat-error>
            </mat-form-field>

            <mat-form-field appearance="outline" class="full-width">
              <mat-label>Confirm Password</mat-label>
              <input matInput [type]="hidePassword() ? 'password' : 'text'" formControlName="confirmPassword">
              <mat-error *ngIf="registerForm.get('confirmPassword')?.hasError('mustMatch')">Passwords must match</mat-error>
            </mat-form-field>

            <button mat-raised-button color="primary" class="full-width" type="submit" [disabled]="registerForm.invalid || isLoading()">
              Register
            </button>
          </form>
        </mat-card-content>

        <mat-card-footer>
          <div class="auth-footer">
            Already have an account? <a routerLink="/auth/login">Login</a>
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
      min-height: 100vh;
      background-color: #f5f5f5;
      padding: 20px;
    }
    .auth-card {
      width: 100%;
      max-width: 500px;
    }
    .full-width {
      width: 100%;
      margin-bottom: 8px;
    }
    .row {
      display: flex;
      gap: 16px;
    }
    .col {
      flex: 1;
    }
    .auth-footer {
      padding: 16px;
      text-align: center;
    }
  `]
})
export class RegisterComponent {
  private fb = inject(FormBuilder);
  private authService = inject(AuthService);
  private router = inject(Router);

  roles = UserRole;
  hidePassword = signal(true);
  isLoading = signal(false);

  registerForm = this.fb.group({
    fullName: ['', [Validators.required, Validators.maxLength(100)]],
    email: ['', [Validators.required, Validators.email]],
    role: [UserRole.Employee, [Validators.required]],
    department: [''],
    designation: [''],
    password: ['', [Validators.required, CustomValidators.passwordPattern()]],
    confirmPassword: ['', [Validators.required]]
  }, {
    validators: [CustomValidators.match('password', 'confirmPassword')]
  });

  onSubmit(): void {
    if (this.registerForm.valid) {
      this.isLoading.set(true);
      this.authService.register(this.registerForm.value as any).subscribe({
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
