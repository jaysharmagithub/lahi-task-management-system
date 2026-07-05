import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';

@Component({
  selector: 'app-not-found',
  standalone: true,
  imports: [CommonModule, RouterModule, MatButtonModule, MatIconModule],
  template: `
    <div class="container">
      <mat-icon class="large-icon">error_outline</mat-icon>
      <h1>404</h1>
      <p>The page you're looking for doesn't exist.</p>
      <button mat-raised-button color="primary" routerLink="/">Go Home</button>
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
      color: #f44336;
    }
    h1 { font-size: 72px; margin: 0; }
    p { font-size: 24px; margin-bottom: 32px; color: rgba(0,0,0,0.6); }
  `]
})
export class NotFoundComponent {}
