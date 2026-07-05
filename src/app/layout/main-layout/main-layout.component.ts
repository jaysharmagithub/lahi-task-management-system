import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { BreakpointObserver, Breakpoints } from '@angular/cdk/layout';
import { RouterOutlet, RouterModule } from '@angular/router';
import { MatSidenavModule } from '@angular/material/sidenav';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatListModule } from '@angular/material/list';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatMenuModule } from '@angular/material/menu';
import { MatBadgeModule } from '@angular/material/badge';
import { MatDividerModule } from '@angular/material/divider';
import { map, shareReplay } from 'rxjs/operators';
import { AuthService } from '../../core/services/auth.service';
import { NotificationService } from '../../core/services/notification.service';

@Component({
  selector: 'app-main-layout',
  standalone: true,
  imports: [
    CommonModule,
    RouterOutlet,
    RouterModule,
    MatSidenavModule,
    MatToolbarModule,
    MatListModule,
    MatIconModule,
    MatButtonModule,
    MatMenuModule,
    MatBadgeModule,
    MatDividerModule
  ],
  template: `
    <mat-sidenav-container class="sidenav-container">
      <mat-sidenav #drawer class="sidenav" fixedInViewport
          [attr.role]="(isHandset$ | async) ? 'dialog' : 'navigation'"
          [mode]="(isHandset$ | async) ? 'over' : 'side'"
          [opened]="(isHandset$ | async) === false">
        <mat-toolbar color="primary">Menu</mat-toolbar>
        <mat-nav-list>
          <a mat-list-item routerLink="/dashboard" routerLinkActive="active-link">
            <mat-icon matListItemIcon>dashboard</mat-icon>
            <span matListItemTitle>Dashboard</span>
          </a>
          <a mat-list-item routerLink="/tasks" routerLinkActive="active-link">
            <mat-icon matListItemIcon>assignment</mat-icon>
            <span matListItemTitle>Tasks</span>
          </a>
          <a *ngIf="isAdmin()" mat-list-item routerLink="/employees" routerLinkActive="active-link">
            <mat-icon matListItemIcon>people</mat-icon>
            <span matListItemTitle>Employees</span>
          </a>
          <a *ngIf="isAdmin()" mat-list-item routerLink="/reports" routerLinkActive="active-link">
            <mat-icon matListItemIcon>bar_chart</mat-icon>
            <span matListItemTitle>Reports</span>
          </a>
        </mat-nav-list>
      </mat-sidenav>
      
      <mat-sidenav-content>
        <mat-toolbar color="primary">
          <button
            type="button"
            aria-label="Toggle sidenav"
            mat-icon-button
            (click)="drawer.toggle()"
            *ngIf="isHandset$ | async">
            <mat-icon aria-label="Side nav toggle icon">menu</mat-icon>
          </button>
          <span>Lahi Task Management</span>
          
          <span class="spacer"></span>
          
          <button mat-icon-button class="nav-icon" aria-label="Notifications" routerLink="/notifications">
            <mat-icon [matBadge]="unreadCount()" matBadgeColor="warn" [matBadgeHidden]="unreadCount() === 0">notifications</mat-icon>
          </button>
          
          <button mat-button [matMenuTriggerFor]="userMenu" class="user-profile">
            <mat-icon>account_circle</mat-icon>
            <span class="username" *ngIf="!(isHandset$ | async)">{{ currentUser()?.fullName }}</span>
          </button>
          
          <mat-menu #userMenu="matMenu">
            <div class="menu-header">
              <div class="menu-user-name">{{ currentUser()?.fullName }}</div>
              <div class="menu-user-email">{{ currentUser()?.email }}</div>
            </div>
            <mat-divider></mat-divider>
            <button mat-menu-item (click)="onLogout()">
              <mat-icon>exit_to_app</mat-icon>
              <span>Logout</span>
            </button>
          </mat-menu>
        </mat-toolbar>
        
        <main class="content">
          <router-outlet></router-outlet>
        </main>
      </mat-sidenav-content>
    </mat-sidenav-container>
  `,
  styles: [`
    .sidenav-container { height: 100vh; }
    .sidenav { width: 240px; }
    .active-link { background: rgba(0, 0, 0, 0.04); color: #3f51b5; font-weight: 500; }
    .spacer { flex: 1 1 auto; }
    .content { padding: 24px; min-height: calc(100vh - 64px); background-color: #fafafa; }
    .user-profile { display: flex; align-items: center; gap: 8px; }
    .menu-header { padding: 16px; outline: none; }
    .menu-user-name { font-weight: 500; font-size: 14px; }
    .menu-user-email { font-size: 12px; color: rgba(0, 0, 0, 0.54); }
  `]
})
export class MainLayoutComponent implements OnInit {
  private breakpointObserver = inject(BreakpointObserver);
  private authService = inject(AuthService);
  private notificationService = inject(NotificationService);

  currentUser = this.authService.currentUser;
  isAdmin = this.authService.isAdmin;
  unreadCount = this.notificationService.unreadCount;

  isHandset$ = this.breakpointObserver.observe(Breakpoints.Handset)
    .pipe(
      map(result => result.matches),
      shareReplay()
    );

  ngOnInit(): void {
    // Initial fetch to sync badge
    this.notificationService.getAll().subscribe();
  }

  onLogout(): void {
    this.authService.logout();
  }
}
