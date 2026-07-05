import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { MatDialogModule, MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatSelectModule } from '@angular/material/select';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { TaskService } from '../task.service';
import { EmployeeService } from '../../employees/employee.service';
import { TaskDto, TaskStatus, TaskPriority } from '../tasks.model';
import { EmployeeDto } from '../../employees/employees.model';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-task-dialog',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatSelectModule,
    MatDatepickerModule,
    MatIconModule,
    MatProgressBarModule
  ],
  template: `
    <h2 mat-dialog-title>{{ isEdit ? 'Edit Task' : 'Create Task' }}</h2>
    <form [formGroup]="form" (ngSubmit)="onSubmit()">
      <mat-dialog-content>
        <div class="form-row">
          <mat-form-field appearance="outline" class="full-width">
            <mat-label>Title</mat-label>
            <input matInput formControlName="title">
            <mat-error *ngIf="form.get('title')?.hasError('required')">Title is required</mat-error>
          </mat-form-field>
        </div>

        <div class="form-row">
          <mat-form-field appearance="outline" class="full-width">
            <mat-label>Description</mat-label>
            <textarea matInput formControlName="description" rows="3"></textarea>
          </mat-form-field>
        </div>

        <div class="form-row">
          <mat-form-field appearance="outline" class="half-width">
            <mat-label>Assigned To</mat-label>
            <mat-select formControlName="assignedToId">
              <mat-option *ngFor="let emp of employees()" [value]="emp.id">{{emp.fullName}}</mat-option>
            </mat-select>
            <mat-error *ngIf="form.get('assignedToId')?.hasError('required')">Assignee is required</mat-error>
          </mat-form-field>

          <mat-form-field appearance="outline" class="half-width">
            <mat-label>Priority</mat-label>
            <mat-select formControlName="priority">
              <mat-option *ngFor="let p of priorityOptions" [value]="p.value">{{p.label}}</mat-option>
            </mat-select>
          </mat-form-field>
        </div>

        <div class="form-row">
          <mat-form-field appearance="outline" class="half-width">
            <mat-label>Status</mat-label>
            <mat-select formControlName="status">
              <mat-option *ngFor="let s of statusOptions" [value]="s.value">{{s.label}}</mat-option>
            </mat-select>
          </mat-form-field>
        </div>

        <div class="form-row">
          <mat-form-field appearance="outline" class="half-width">
            <mat-label>Start Date</mat-label>
            <input matInput [matDatepicker]="startPicker" formControlName="startDate">
            <mat-datepicker-toggle matSuffix [for]="startPicker"></mat-datepicker-toggle>
            <mat-datepicker #startPicker></mat-datepicker>
          </mat-form-field>

          <mat-form-field appearance="outline" class="half-width">
            <mat-label>Due Date</mat-label>
            <input matInput [matDatepicker]="duePicker" formControlName="dueDate">
            <mat-datepicker-toggle matSuffix [for]="duePicker"></mat-datepicker-toggle>
            <mat-datepicker #duePicker></mat-datepicker>
            <mat-error *ngIf="form.hasError('dateRange')">Due date must be after start date</mat-error>
          </mat-form-field>
        </div>

        <div class="attachment-section" *ngIf="isEdit">
          <h3>Attachment (PDF, JPG, PNG - Max 5MB)</h3>
          <div *ngIf="data && data.attachmentFileName" class="file-info">
            <mat-icon>insert_drive_file</mat-icon>
            <span>{{data.attachmentFileName}}</span>
          </div>
          <input type="file" #fileInput style="display: none" (change)="onFileSelected($event)" accept=".pdf,.jpg,.png">
          <button type="button" mat-stroked-button (click)="fileInput.click()" [disabled]="isUploading() || isCompletedLocked()">
            <mat-icon>attach_file</mat-icon> {{ (data && data.attachmentFileName) ? 'Change File' : 'Upload File' }}
          </button>
          <mat-progress-bar *ngIf="isUploading()" mode="determinate" [value]="uploadProgress()" style="margin-top: 8px"></mat-progress-bar>
        </div>

      </mat-dialog-content>

      <mat-dialog-actions align="end">
        <button mat-button type="button" (click)="dialogRef.close()">Cancel</button>
        <button mat-raised-button color="primary" type="submit" [disabled]="form.invalid || isCompletedLocked()">
          {{ isEdit ? 'Update' : 'Create' }}
        </button>
      </mat-dialog-actions>
    </form>
  `,
  styles: [`
    .form-row { margin-bottom: 8px; display: flex; gap: 16px; }
    .full-width { width: 100%; }
    .half-width { flex: 1; }
    .attachment-section { margin-top: 16px; padding-top: 16px; border-top: 1px solid #eee; }
    .file-info { display: flex; align-items: center; gap: 8px; margin-bottom: 8px; color: #3f51b5; font-weight: 500; }
  `]
})
export class TaskDialogComponent implements OnInit {
  private fb = inject(FormBuilder);
  private taskService = inject(TaskService);
  private employeeService = inject(EmployeeService);
  private authService = inject(AuthService);
  public dialogRef = inject(MatDialogRef<TaskDialogComponent>);
  public data = inject<TaskDto>(MAT_DIALOG_DATA);

  form!: FormGroup;
  isEdit = false;
  isUploading = signal(false);
  uploadProgress = signal(0);
  employees = signal<EmployeeDto[]>([]);
  
  statusOptions = [
    { value: TaskStatus.Pending, label: 'Pending' },
    { value: TaskStatus.InProgress, label: 'In Progress' },
    { value: TaskStatus.Completed, label: 'Completed' }
  ];

  priorityOptions = [
    { value: TaskPriority.Low, label: 'Low' },
    { value: TaskPriority.Medium, label: 'Medium' },
    { value: TaskPriority.High, label: 'High' }
  ];

  ngOnInit(): void {
    this.isEdit = !!this.data;
    this.loadEmployees();

    this.form = this.fb.group({
      title: [this.data?.title || '', [Validators.required, Validators.maxLength(200)]],
      description: [this.data?.description || '', [Validators.maxLength(2000)]],
      assignedToId: [this.data?.assignedToId || '', [Validators.required]],
      priority: [this.data?.priority || TaskPriority.Medium],
      status: [this.data?.status || TaskStatus.Pending],
      startDate: [this.data?.startDate ? new Date(this.data.startDate) : new Date(), [Validators.required]],
      dueDate: [this.data?.dueDate ? new Date(this.data.dueDate) : new Date(), [Validators.required]]
    }, { validators: this.dateRangeValidator });

    if (this.isCompletedLocked()) {
      this.form.disable();
    }
  }

  isCompletedLocked(): boolean {
    // Admins can ALWAYS edit. Employees can't edit COMPLETED tasks.
    return !this.authService.isAdmin() && this.isEdit && this.data?.status === TaskStatus.Completed;
  }

  private loadEmployees(): void {
    this.employeeService.getLookup().subscribe(res => {
      this.employees.set(res as any);
    });
  }

  private dateRangeValidator(group: FormGroup): { [key: string]: boolean } | null {
    const start = group.get('startDate')?.value;
    const due = group.get('dueDate')?.value;
    return start && due && new Date(due) < new Date(start) ? { dateRange: true } : null;
  }

  onFileSelected(event: any): void {
    const file = event.target.files[0];
    if (file && this.data) {
      if (file.size > 5 * 1024 * 1024) {
        alert('File size exceeds 5MB');
        return;
      }
      this.isUploading.set(true);
      this.uploadProgress.set(30); // Visual indicator
      this.taskService.uploadAttachment(this.data.id, file).subscribe({
        next: (updatedTask) => {
          this.data = updatedTask;
          this.uploadProgress.set(100);
          setTimeout(() => {
            this.isUploading.set(false);
            this.uploadProgress.set(0);
          }, 500);
        },
        error: () => {
          this.isUploading.set(false);
          this.uploadProgress.set(0);
        }
      });
    }
  }

  onSubmit(): void {
    if (this.form.valid) {
      const val = this.form.getRawValue(); // Use getRawValue to include disabled fields if needed
      const obs = this.isEdit 
        ? this.taskService.update(this.data!.id, val)
        : this.taskService.create(val);

      obs.subscribe(() => this.dialogRef.close(true));
    }
  }
}
