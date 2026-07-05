import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router'; // Added this
import { MatGridListModule } from '@angular/material/grid-list';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { AuthService } from '../../core/services/auth.service';
import { ReportService } from '../../core/services/report.service';
import { AdminDashboardStats, EmployeeDashboardStats } from '../../core/models/report.model';

interface StatCard {
  title: string;
  value: number;
  icon: string;
  color: string;
}

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule, // Added this to enable routerLink
    MatGridListModule,
    MatCardModule,
    MatIconModule,
    MatButtonModule,
    MatProgressBarModule
  ],
  template: `
    <div class="dashboard-container">
      <header class="dashboard-header">
        <h1>Dashboard</h1>
        <p>Welcome back, {{ currentUser()?.fullName }}</p>
      </header>

      <mat-progress-bar *ngIf="isLoading()" mode="indeterminate" class="loader"></mat-progress-bar>

      <mat-grid-list [cols]="(stats().length)" rowHeight="140px" gutterSize="16px">
        <mat-grid-tile *ngFor="let stat of stats()">
          <mat-card class="stat-card" [style.border-left-color]="stat.color">
            <mat-card-content>
              <div class="stat-header">
                <span class="stat-title">{{ stat.title }}</span>
                <mat-icon [style.color]="stat.color">{{ stat.icon }}</mat-icon>
              </div>
              <div class="stat-value">{{ stat.value }}</div>
            </mat-card-content>
          </mat-card>
        </mat-grid-tile>
      </mat-grid-list>

      <div class="dashboard-content" *ngIf="!isLoading()">
        <mat-card class="welcome-card">
          <mat-card-header>
            <mat-card-title>Quick Summary</mat-card-title>
          </mat-card-header>
          <mat-card-content>
             <p *ngIf="isAdmin()">You have {{ adminStats()?.totalTasks }} total tasks across {{ adminStats()?.totalEmployees }} employees.</p>
             <p *ngIf="!isAdmin()">You have {{ employeeStats()?.pendingTasks }} pending tasks requiring your attention.</p>
          </mat-card-content>
          <mat-card-actions>
            <button mat-raised-button color="primary" routerLink="/tasks">View All Tasks</button>
          </mat-card-actions>
        </mat-card>
      </div>
    </div>
  `,
  styles: [`
    .dashboard-container { display: flex; flex-direction: column; gap: 24px; }
    .loader { margin-bottom: 16px; }
    .stat-card { width: 100%; height: 100%; border-left: 4px solid; }
    .stat-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 8px; }
    .stat-title { font-size: 14px; font-weight: 500; color: rgba(0, 0, 0, 0.6); text-transform: uppercase; }
    .stat-value { font-size: 32px; font-weight: 700; }
    .dashboard-content { margin-top: 24px; }
    .welcome-card { padding: 16px; }
  `]
})
export class DashboardComponent implements OnInit {
  private authService = inject(AuthService);
  private reportService = inject(ReportService);

  currentUser = this.authService.currentUser;
  isAdmin = this.authService.isAdmin;

  isLoading = signal(false);
  adminStats = signal<AdminDashboardStats | null>(null);
  employeeStats = signal<EmployeeDashboardStats | null>(null);
  stats = signal<StatCard[]>([]);

  ngOnInit(): void {
    this.loadData();
  }

  private loadData(): void {
    this.isLoading.set(true);
    if (this.isAdmin()) {
      this.reportService.getAdminDashboard().subscribe({
        next: (data) => {
          this.adminStats.set(data);
          this.stats.set([
            { title: 'Employees', value: data.totalEmployees, icon: 'people', color: '#3f51b5' },
            { title: 'Total Tasks', value: data.totalTasks, icon: 'assignment', color: '#2196f3' },
            { title: 'Completed', value: data.completedTasks, icon: 'check_circle', color: '#4caf50' },
            { title: 'Pending', value: data.pendingTasks, icon: 'schedule', color: '#ff9800' },
            { title: 'Overdue', value: data.overdueTasks, icon: 'warning', color: '#f44336' }
          ]);
          this.isLoading.set(false);
        },
        error: () => this.isLoading.set(false)
      });
    } else {
      this.reportService.getEmployeeDashboard().subscribe({
        next: (data) => {
          this.employeeStats.set(data);
          this.stats.set([
            { title: 'My Tasks', value: data.myTasks, icon: 'assignment', color: '#3f51b5' },
            { title: 'Completed', value: data.completedTasks, icon: 'check_circle', color: '#4caf50' },
            { title: 'Pending', value: data.pendingTasks, icon: 'schedule', color: '#ff9800' },
            { title: 'Overdue', value: data.overdueTasks, icon: 'warning', color: '#f44336' }
          ]);
          this.isLoading.set(false);
        },
        error: () => this.isLoading.set(false)
      });
    }
  }
}
