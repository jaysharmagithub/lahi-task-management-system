import { Component, OnInit, inject, signal, ViewChild, AfterViewInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatTableModule } from '@angular/material/table';
import { MatPaginatorModule, MatPaginator } from '@angular/material/paginator';
import { MatSortModule, MatSort } from '@angular/material/sort';
import { MatInputModule } from '@angular/material/input';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatChipsModule } from '@angular/material/chips';
import { MatSelectModule } from '@angular/material/select';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatCardModule } from '@angular/material/card';
import { MatTooltipModule } from '@angular/material/tooltip';
import { FormControl, ReactiveFormsModule, FormGroup } from '@angular/forms';
import { debounceTime, distinctUntilChanged, merge, startWith, switchMap, catchError, of } from 'rxjs';
import { TaskService } from '../task.service';
import { TaskDto, TaskStatus, TaskPriority } from '../tasks.model';
import { TaskDialogComponent } from '../task-dialog/task-dialog.component';
import { ConfirmDialogComponent } from '../../../shared/components/confirm-dialog/confirm-dialog.component';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-task-list',
  standalone: true,
  imports: [
    CommonModule,
    MatTableModule,
    MatPaginatorModule,
    MatSortModule,
    MatInputModule,
    MatFormFieldModule,
    MatButtonModule,
    MatIconModule,
    MatChipsModule,
    MatSelectModule,
    MatDialogModule,
    MatCardModule,
    MatTooltipModule,
    ReactiveFormsModule
  ],
  template: `
    <div class="page-container">
      <header class="page-header">
        <div>
          <h1>Tasks</h1>
          <p>Track and manage project tasks.</p>
        </div>
        <button mat-raised-button color="primary" (click)="openDialog()">
          <mat-icon>add</mat-icon> Create Task
        </button>
      </header>

      <mat-card class="filter-card">
        <form [formGroup]="filterForm" class="filter-form">
          <mat-form-field appearance="outline" class="search-field">
            <mat-label>Search tasks</mat-label>
            <input matInput formControlName="search" placeholder="Search by title...">
            <mat-icon matPrefix>search</mat-icon>
          </mat-form-field>

          <mat-form-field appearance="outline">
            <mat-label>Status</mat-label>
            <mat-select formControlName="status">
              <mat-option [value]="null">All Statuses</mat-option>
              <mat-option *ngFor="let s of statusOptions" [value]="s.value">{{s.label}}</mat-option>
            </mat-select>
          </mat-form-field>

          <mat-form-field appearance="outline">
            <mat-label>Priority</mat-label>
            <mat-select formControlName="priority">
              <mat-option [value]="null">All Priorities</mat-option>
              <mat-option *ngFor="let p of priorityOptions" [value]="p.value">{{p.label}}</mat-option>
            </mat-select>
          </mat-form-field>
        </form>
      </mat-card>

      <div class="table-container mat-elevation-z2">
        <table mat-table [dataSource]="dataSource" matSort matSortActive="createdAt" matSortDirection="desc">
          
          <ng-container matColumnDef="title">
            <th mat-header-cell *matHeaderCellDef mat-sort-header>Title</th>
            <td mat-cell *matCellDef="let element">
              <div class="title-cell">
                <span class="task-title">{{element.title}}</span>
                <mat-icon *ngIf="element.attachmentFileName" class="attach-icon" matTooltip="Has attachment">attach_file</mat-icon>
              </div>
            </td>
          </ng-container>

          <ng-container matColumnDef="assignedToName">
            <th mat-header-cell *matHeaderCellDef mat-sort-header>Assigned To</th>
            <td mat-cell *matCellDef="let element">{{element.assignedToName}}</td>
          </ng-container>

          <ng-container matColumnDef="dueDate">
            <th mat-header-cell *matHeaderCellDef mat-sort-header>Due Date</th>
            <td mat-cell *matCellDef="let element" [class.overdue]="isOverdue(element.dueDate)">
              {{element.dueDate | date:'mediumDate'}}
            </td>
          </ng-container>

          <ng-container matColumnDef="priority">
            <th mat-header-cell *matHeaderCellDef mat-sort-header>Priority</th>
            <td mat-cell *matCellDef="let element">
              <span class="badge" [ngClass]="'priority-' + getPriorityLabel(element.priority).toLowerCase()">
                {{getPriorityLabel(element.priority)}}
              </span>
            </td>
          </ng-container>

          <ng-container matColumnDef="status">
            <th mat-header-cell *matHeaderCellDef mat-sort-header>Status</th>
            <td mat-cell *matCellDef="let element">
              <span class="badge" [ngClass]="'status-' + getStatusLabel(element.status).toLowerCase()">
                {{getStatusLabel(element.status)}}
              </span>
            </td>
          </ng-container>

          <ng-container matColumnDef="actions">
            <th mat-header-cell *matHeaderCellDef>Actions</th>
            <td mat-cell *matCellDef="let element">
              <button mat-icon-button color="primary" (click)="openDialog(element)" matTooltip="Edit">
                <mat-icon>edit</mat-icon>
              </button>
              <button *ngIf="isAdmin()" mat-icon-button color="warn" (click)="deleteTask(element)" matTooltip="Delete">
                <mat-icon>delete</mat-icon>
              </button>
            </td>
          </ng-container>

          <tr mat-header-row *matHeaderRowDef="displayedColumns"></tr>
          <tr mat-row *matRowDef="let row; columns: displayedColumns;"></tr>

          <tr class="mat-row" *matNoDataRow>
            <td class="mat-cell" colspan="6">No tasks found.</td>
          </tr>
        </table>

        <mat-paginator [length]="totalCount()" [pageSize]="10" [pageSizeOptions]="[5, 10, 25, 50]"></mat-paginator>
      </div>
    </div>
  `,
  styles: [`
    .page-container { padding: 8px; }
    .page-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 24px; }
    .filter-card { margin-bottom: 16px; padding: 16px; }
    .filter-form { display: flex; gap: 16px; align-items: center; flex-wrap: wrap; }
    .search-field { flex: 1; min-width: 250px; }
    .table-container { background: white; border-radius: 8px; overflow: hidden; }
    table { width: 100%; }
    .overdue { color: #c62828; font-weight: 500; }
    .title-cell { display: flex; align-items: center; gap: 4px; }
    .attach-icon { font-size: 16px; width: 16px; height: 16px; color: #757575; }
  `]
})
export class TaskListComponent implements OnInit, AfterViewInit {
  private taskService = inject(TaskService);
  private authService = inject(AuthService);
  private dialog = inject(MatDialog);
  private snackBar = inject(MatSnackBar);

  isAdmin = this.authService.isAdmin;

  statusOptions = [
    { value: TaskStatus.Pending, label: 'Pending' },
    { value: TaskStatus.InProgress, label: 'In Progress' },
    { value: TaskStatus.Completed, label: 'Completed' },
    { value: TaskStatus.Overdue, label: 'Overdue' }
  ];

  priorityOptions = [
    { value: TaskPriority.Low, label: 'Low' },
    { value: TaskPriority.Medium, label: 'Medium' },
    { value: TaskPriority.High, label: 'High' },
    { value: TaskPriority.Critical, label: 'Critical' }
  ];

  displayedColumns: string[] = ['title', 'assignedToName', 'dueDate', 'priority', 'status', 'actions'];
  dataSource: TaskDto[] = [];
  totalCount = signal(0);
  isLoading = signal(true);

  filterForm = new FormGroup({
    search: new FormControl(''),
    status: new FormControl<TaskStatus | null>(null),
    priority: new FormControl<TaskPriority | null>(null)
  });

  @ViewChild(MatPaginator) paginator!: MatPaginator;
  @ViewChild(MatSort) sort!: MatSort;

  ngOnInit(): void {}

  ngAfterViewInit(): void {
    this.filterForm.valueChanges.pipe(debounceTime(400)).subscribe(() => {
      this.paginator.pageIndex = 0;
    });

    merge(this.sort.sortChange, this.paginator.page, this.filterForm.valueChanges.pipe(debounceTime(400)))
      .pipe(
        startWith({}),
        switchMap(() => {
          this.isLoading.set(true);
          const filters = this.filterForm.value;
          return this.taskService.getAll({
            page: this.paginator.pageIndex + 1,
            pageSize: this.paginator.pageSize,
            search: filters.search || undefined,
            status: filters.status || undefined,
            priority: filters.priority || undefined,
            sortBy: this.sort.active,
            sortDirection: this.sort.direction as 'asc' | 'desc'
          }).pipe(catchError(() => of(null)));
        })
      ).subscribe(result => {
        this.isLoading.set(false);
        if (result) {
          this.dataSource = result.items;
          this.totalCount.set(result.totalCount);
        }
      });
  }

  getPriorityLabel(value: number): string {
    return this.priorityOptions.find(o => o.value === value)?.label || 'Unknown';
  }

  getStatusLabel(value: number): string {
    return this.statusOptions.find(o => o.value === value)?.label || 'Unknown';
  }

  isOverdue(date: string): boolean {
    return new Date(date) < new Date();
  }

  openDialog(task?: TaskDto): void {
    const dialogRef = this.dialog.open(TaskDialogComponent, {
      width: '650px',
      data: task
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) this.refreshData();
    });
  }

  deleteTask(task: TaskDto): void {
    const dialogRef = this.dialog.open(ConfirmDialogComponent, {
      data: {
        title: 'Delete Task',
        message: `Are you sure you want to delete "${task.title}"?`
      }
    });

    dialogRef.afterClosed().subscribe(confirm => {
      if (confirm) {
        this.taskService.delete(task.id).subscribe(() => {
          this.snackBar.open('Task deleted', 'Close', { duration: 3000 });
          this.refreshData();
        });
      }
    });
  }

  private refreshData(): void {
    this.paginator.page.emit();
  }
}
