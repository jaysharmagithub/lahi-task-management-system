import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';

@Component({
  selector: 'app-unauthorized',
  standalone: true,
  imports: [CommonModule, RouterModule, MatButtonModule, MatIconModule],
  template: `
    <div class="container">
      <mat-icon class="large-icon">lock_outline</mat-icon>
      <h1>Unauthorized Access</h1>
      <p>You do not have permission to view this page.</p>
      <button mat-raised-button color="primary" routerLink="/">Go to Dashboard</button>
    </div>
  `,
  styles: [`
    .container {
      display: flex;
      flex-direction: column;
      align-items: center;
      justify-content: center;
      height: 100vh;
      text-align: center;
    }
    .large-icon {
      font-size: 100px;
      width: 100px;
      height: 100px;
      margin-bottom: 24px;
      color: #ff9800;
    }
    h1 { font-size: 48px; margin: 0; }
    p { font-size: 20px; margin-bottom: 32px; color: rgba(0,0,0,0.6); }
  `]
})
export class UnauthorizedComponent {}
