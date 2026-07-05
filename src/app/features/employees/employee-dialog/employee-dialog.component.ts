import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { MatDialogModule, MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatSelectModule } from '@angular/material/select';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { EmployeeService } from '../employee.service';
import { EmployeeDto } from '../employees.model';

@Component({
  selector: 'app-employee-dialog',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatSelectModule,
    MatSlideToggleModule
  ],
  template: `
    <h2 mat-dialog-title>{{ isEdit ? 'Edit Employee' : 'Add Employee' }}</h2>
    <form [formGroup]="form" (ngSubmit)="onSubmit()">
      <mat-dialog-content>
        <div class="form-row">
          <mat-form-field appearance="outline" class="full-width">
            <mat-label>Full Name</mat-label>
            <input matInput formControlName="fullName" placeholder="John Doe">
            <mat-error *ngIf="form.get('fullName')?.hasError('required')">Name is required</mat-error>
          </mat-form-field>
        </div>

        <div class="form-row">
          <mat-form-field appearance="outline" class="full-width">
            <mat-label>Email</mat-label>
            <input matInput formControlName="email" type="email" placeholder="john@example.com">
            <mat-error *ngIf="form.get('email')?.hasError('required')">Email is required</mat-error>
            <mat-error *ngIf="form.get('email')?.hasError('email')">Invalid email</mat-error>
          </mat-form-field>
        </div>

        <div class="form-row" *ngIf="!isEdit">
          <mat-form-field appearance="outline" class="full-width">
            <mat-label>Password</mat-label>
            <input matInput formControlName="password" type="password">
            <mat-error *ngIf="form.get('password')?.hasError('required')">Password is required</mat-error>
          </mat-form-field>
        </div>

        <div class="form-row">
          <mat-form-field appearance="outline" class="half-width">
            <mat-label>Department</mat-label>
            <input matInput formControlName="department">
          </mat-form-field>
          <mat-form-field appearance="outline" class="half-width">
            <mat-label>Designation</mat-label>
            <input matInput formControlName="designation">
          </mat-form-field>
        </div>

        <div class="form-row" *ngIf="isEdit">
          <mat-slide-toggle formControlName="isActive">Active</mat-slide-toggle>
        </div>
      </mat-dialog-content>

      <mat-dialog-actions align="end">
        <button mat-button type="button" (click)="dialogRef.close()">Cancel</button>
        <button mat-raised-button color="primary" type="submit" [disabled]="form.invalid">
          {{ isEdit ? 'Update' : 'Create' }}
        </button>
      </mat-dialog-actions>
    </form>
  `,
  styles: [`
    .form-row { margin-bottom: 12px; display: flex; gap: 16px; }
    .full-width { width: 100%; }
    .half-width { flex: 1; }
  `]
})
export class EmployeeDialogComponent implements OnInit {
  private fb = inject(FormBuilder);
  private employeeService = inject(EmployeeService);
  public dialogRef = inject(MatDialogRef<EmployeeDialogComponent>);
  public data = inject<EmployeeDto>(MAT_DIALOG_DATA);

  form!: FormGroup;
  isEdit = false;

  ngOnInit(): void {
    this.isEdit = !!this.data;
    this.form = this.fb.group({
      fullName: [this.data?.fullName || '', [Validators.required]],
      email: [this.data?.email || '', [Validators.required, Validators.email]],
      department: [this.data?.department || ''],
      designation: [this.data?.designation || ''],
      isActive: [this.data?.isActive ?? true]
    });

    if (!this.isEdit) {
      this.form.addControl('password', this.fb.control('', [Validators.required, Validators.minLength(8)]));
    }
  }

  onSubmit(): void {
    if (this.form.valid) {
      const request = this.form.value;
      const obs = this.isEdit 
        ? this.employeeService.update(this.data!.id, request)
        : this.employeeService.create(request);

      obs.subscribe({
        next: () => this.dialogRef.close(true),
        error: () => {} // Error handled by interceptor
      });
    }
  }
}
