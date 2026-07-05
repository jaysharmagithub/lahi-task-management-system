import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatTabsModule } from '@angular/material/tabs';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatCardModule } from '@angular/material/card';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatMenuModule } from '@angular/material/menu';
import { ReportService } from '../../core/services/report.service';
import { TaskReportDto, EmployeeReportDto } from '../../core/models/report.model';

@Component({
  selector: 'app-reports',
  standalone: true,
  imports: [
    CommonModule,
    MatTabsModule,
    MatTableModule,
    MatButtonModule,
    MatIconModule,
    MatCardModule,
    MatProgressBarModule,
    MatMenuModule
  ],
  template: `
    <div class="page-container">
      <header class="page-header">
        <h1>Reports</h1>
        <p>Export and analyze task performance.</p>
      </header>

      <mat-card class="report-card">
        <mat-tab-group (selectedTabChange)="onTabChange($event.index)">
          
          <mat-tab label="Completed Tasks">
            <ng-template matTabContent>
              <div class="tab-actions">
                <button mat-stroked-button [matMenuTriggerFor]="exportMenu">
                  <mat-icon>download</mat-icon> Export
                </button>
                <mat-menu #exportMenu="matMenu">
                  <button mat-menu-item (click)="export('completed', 'excel')">Excel (.xlsx)</button>
                  <button mat-menu-item (click)="export('completed', 'csv')">CSV (.csv)</button>
                </mat-menu>
              </div>
              <div class="table-container">
                <table mat-table [dataSource]="taskData()">
                  <ng-container matColumnDef="title">
                    <th mat-header-cell *matHeaderCellDef>Task Title</th>
                    <td mat-cell *matCellDef="let row">{{row.title}}</td>
                  </ng-container>
                  <ng-container matColumnDef="assignedTo">
                    <th mat-header-cell *matHeaderCellDef>Assigned To</th>
                    <td mat-cell *matCellDef="let row">{{row.assignedTo}}</td>
                  </ng-container>
                  <ng-container matColumnDef="dueDate">
                    <th mat-header-cell *matHeaderCellDef>Due Date</th>
                    <td mat-cell *matCellDef="let row">{{row.dueDate | date}}</td>
                  </ng-container>
                  <ng-container matColumnDef="priority">
                    <th mat-header-cell *matHeaderCellDef>Priority</th>
                    <td mat-cell *matCellDef="let row">{{row.priority}}</td>
                  </ng-container>

                  <tr mat-header-row *matHeaderRowDef="taskColumns"></tr>
                  <tr mat-row *matRowDef="let row; columns: taskColumns;"></tr>
                </table>
              </div>
            </ng-template>
          </mat-tab>

          <mat-tab label="Pending Tasks">
            <ng-template matTabContent>
              <div class="tab-actions">
                <button mat-stroked-button [matMenuTriggerFor]="exportMenu">
                  <mat-icon>download</mat-icon> Export
                </button>
                <mat-menu #exportMenu="matMenu">
                  <button mat-menu-item (click)="export('pending', 'excel')">Excel (.xlsx)</button>
                  <button mat-menu-item (click)="export('pending', 'csv')">CSV (.csv)</button>
                </mat-menu>
              </div>
              <div class="table-container">
                <table mat-table [dataSource]="taskData()">
                  <ng-container matColumnDef="title">
                    <th mat-header-cell *matHeaderCellDef>Task Title</th>
                    <td mat-cell *matCellDef="let row">{{row.title}}</td>
                  </ng-container>
                  <ng-container matColumnDef="assignedTo">
                    <th mat-header-cell *matHeaderCellDef>Assigned To</th>
                    <td mat-cell *matCellDef="let row">{{row.assignedTo}}</td>
                  </ng-container>
                  <ng-container matColumnDef="dueDate">
                    <th mat-header-cell *matHeaderCellDef>Due Date</th>
                    <td mat-cell *matCellDef="let row">{{row.dueDate | date}}</td>
                  </ng-container>
                  <ng-container matColumnDef="priority">
                    <th mat-header-cell *matHeaderCellDef>Priority</th>
                    <td mat-cell *matCellDef="let row">{{row.priority}}</td>
                  </ng-container>

                  <tr mat-header-row *matHeaderRowDef="taskColumns"></tr>
                  <tr mat-row *matRowDef="let row; columns: taskColumns;"></tr>
                </table>
              </div>
            </ng-template>
          </mat-tab>

          <mat-tab label="Employee Performance">
            <ng-template matTabContent>
              <div class="tab-actions">
                <button mat-stroked-button [matMenuTriggerFor]="exportMenu">
                  <mat-icon>download</mat-icon> Export
                </button>
                <mat-menu #exportMenu="matMenu">
                  <button mat-menu-item (click)="export('employee', 'excel')">Excel (.xlsx)</button>
                  <button mat-menu-item (click)="export('employee', 'csv')">CSV (.csv)</button>
                </mat-menu>
              </div>
              <div class="table-container">
                <table mat-table [dataSource]="employeeData()">
                  <ng-container matColumnDef="name">
                    <th mat-header-cell *matHeaderCellDef>Employee</th>
                    <td mat-cell *matCellDef="let row">{{row.employeeName}}</td>
                  </ng-container>
                  <ng-container matColumnDef="total">
                    <th mat-header-cell *matHeaderCellDef>Total Tasks</th>
                    <td mat-cell *matCellDef="let row">{{row.totalTasks}}</td>
                  </ng-container>
                  <ng-container matColumnDef="completed">
                    <th mat-header-cell *matHeaderCellDef>Completed</th>
                    <td mat-cell *matCellDef="let row">{{row.completedTasks}}</td>
                  </ng-container>
                  <ng-container matColumnDef="pending">
                    <th mat-header-cell *matHeaderCellDef>Pending</th>
                    <td mat-cell *matCellDef="let row">{{row.pendingTasks}}</td>
                  </ng-container>

                  <tr mat-header-row *matHeaderRowDef="employeeColumns"></tr>
                  <tr mat-row *matRowDef="let row; columns: employeeColumns;"></tr>
                </table>
              </div>
            </ng-template>
          </mat-tab>

        </mat-tab-group>
        <mat-progress-bar *ngIf="isLoading()" mode="indeterminate"></mat-progress-bar>
      </mat-card>
    </div>
  `,
  styles: [`
    .page-container { padding: 8px; }
    .page-header { margin-bottom: 24px; }
    .report-card { min-height: 400px; }
    .tab-actions { padding: 16px; display: flex; justify-content: flex-end; }
    .table-container { padding: 0 16px 16px 16px; }
    table { width: 100%; }
  `]
})
export class ReportsComponent {
  private reportService = inject(ReportService);

  isLoading = signal(false);
  taskData = signal<TaskReportDto[]>([]);
  employeeData = signal<EmployeeReportDto[]>([]);

  taskColumns = ['title', 'assignedTo', 'dueDate', 'priority'];
  employeeColumns = ['name', 'total', 'completed', 'pending'];

  constructor() {
    this.loadCompleted();
  }

  onTabChange(index: number): void {
    if (index === 0) this.loadCompleted();
    else if (index === 1) this.loadPending();
    else if (index === 2) this.loadEmployeeWise();
  }

  private loadCompleted(): void {
    this.isLoading.set(true);
    this.reportService.getCompletedTasks().subscribe({
      next: (data) => { this.taskData.set(data); this.isLoading.set(false); },
      error: () => this.isLoading.set(false)
    });
  }

  private loadPending(): void {
    this.isLoading.set(true);
    this.reportService.getPendingTasks().subscribe({
      next: (data) => { this.taskData.set(data); this.isLoading.set(false); },
      error: () => this.isLoading.set(false)
    });
  }

  private loadEmployeeWise(): void {
    this.isLoading.set(true);
    this.reportService.getEmployeeWiseReport().subscribe({
      next: (data) => { this.employeeData.set(data); this.isLoading.set(false); },
      error: () => this.isLoading.set(false)
    });
  }

  export(type: 'completed' | 'pending' | 'employee', format: 'excel' | 'csv'): void {
    const obs = format === 'excel' 
      ? this.reportService.exportToExcel(type)
      : this.reportService.exportToCsv(type);

    obs.subscribe(blob => {
      const url = window.URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.href = url;
      a.download = `${type}-report.${format === 'excel' ? 'xlsx' : 'csv'}`;
      a.click();
      window.URL.revokeObjectURL(url);
    });
  }
}
