import { Component, OnInit, inject, signal, ViewChild, AfterViewInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatTableModule } from '@angular/material/table';
import { MatPaginatorModule, MatPaginator } from '@angular/material/paginator';
import { MatSortModule, MatSort } from '@angular/material/sort';
import { MatInputModule } from '@angular/material/input';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { debounceTime, distinctUntilChanged, merge, startWith, switchMap, catchError, of } from 'rxjs';
import { EmployeeService } from '../employee.service';
import { EmployeeDto } from '../employees.model';
import { EmployeeDialogComponent } from '../employee-dialog/employee-dialog.component';
import { ConfirmDialogComponent } from '../../../shared/components/confirm-dialog/confirm-dialog.component';

@Component({
  selector: 'app-employee-list',
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
    MatTooltipModule,
    MatDialogModule,
    ReactiveFormsModule
  ],
  template: `
    <div class="page-container">
      <header class="page-header">
        <div>
          <h1>Employees</h1>
          <p>Manage your team members and their roles.</p>
        </div>
        <button mat-raised-button color="primary" (click)="openDialog()">
          <mat-icon>add</mat-icon> Add Employee
        </button>
      </header>

      <div class="table-actions">
        <mat-form-field appearance="outline" class="search-field">
          <mat-label>Search employees</mat-label>
          <input matInput [formControl]="searchControl" placeholder="Search by name or email...">
          <mat-icon matPrefix>search</mat-icon>
        </mat-form-field>
      </div>

      <div class="table-container mat-elevation-z2">
        <table mat-table [dataSource]="dataSource" matSort matSortActive="fullName" matSortDirection="asc">
          
          <ng-container matColumnDef="fullName">
            <th mat-header-cell *matHeaderCellDef mat-sort-header>Name</th>
            <td mat-cell *matCellDef="let element">{{element.fullName}}</td>
          </ng-container>

          <ng-container matColumnDef="email">
            <th mat-header-cell *matHeaderCellDef mat-sort-header>Email</th>
            <td mat-cell *matCellDef="let element">{{element.email}}</td>
          </ng-container>

          <ng-container matColumnDef="department">
            <th mat-header-cell *matHeaderCellDef mat-sort-header>Department</th>
            <td mat-cell *matCellDef="let element">{{element.department || 'N/A'}}</td>
          </ng-container>

          <ng-container matColumnDef="designation">
            <th mat-header-cell *matHeaderCellDef mat-sort-header>Designation</th>
            <td mat-cell *matCellDef="let element">{{element.designation || 'N/A'}}</td>
          </ng-container>

          <ng-container matColumnDef="actions">
            <th mat-header-cell *matHeaderCellDef>Actions</th>
            <td mat-cell *matCellDef="let element">
              <button mat-icon-button color="primary" (click)="openDialog(element)" matTooltip="Edit">
                <mat-icon>edit</mat-icon>
              </button>
              <button mat-icon-button color="warn" (click)="deleteEmployee(element)" matTooltip="Delete">
                <mat-icon>delete</mat-icon>
              </button>
            </td>
          </ng-container>

          <tr mat-header-row *matHeaderRowDef="displayedColumns"></tr>
          <tr mat-row *matRowDef="let row; columns: displayedColumns;"></tr>

          <tr class="mat-row" *matNoDataRow>
            <td class="mat-cell" colspan="5">No employees found matching the search "{{searchControl.value}}"</td>
          </tr>
        </table>

        <mat-paginator [length]="totalCount()" [pageSize]="10" [pageSizeOptions]="[5, 10, 25, 100]" aria-label="Select page of employees"></mat-paginator>
      </div>
    </div>
  `,
  styles: [`
    .page-container { padding: 8px; }
    .page-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 24px; }
    .table-actions { margin-bottom: 16px; }
    .search-field { width: 100%; max-width: 400px; }
    .table-container { background: white; border-radius: 8px; overflow: hidden; }
    table { width: 100%; }
  `]
})
export class EmployeeListComponent implements OnInit, AfterViewInit {
  private employeeService = inject(EmployeeService);
  private dialog = inject(MatDialog);
  private snackBar = inject(MatSnackBar);

  displayedColumns: string[] = ['fullName', 'email', 'department', 'designation', 'actions'];
  dataSource: EmployeeDto[] = [];
  totalCount = signal(0);
  isLoading = signal(true);
  searchControl = new FormControl('');

  @ViewChild(MatPaginator) paginator!: MatPaginator;
  @ViewChild(MatSort) sort!: MatSort;

  ngOnInit(): void {}

  ngAfterViewInit(): void {
    // Reset paginator on search
    this.searchControl.valueChanges.pipe(debounceTime(400), distinctUntilChanged()).subscribe(() => {
      this.paginator.pageIndex = 0;
    });

    merge(this.sort.sortChange, this.paginator.page, this.searchControl.valueChanges.pipe(debounceTime(400)))
      .pipe(
        startWith({}),
        switchMap(() => {
          this.isLoading.set(true);
          return this.employeeService.getAll({
            page: this.paginator.pageIndex + 1,
            pageSize: this.paginator.pageSize,
            search: this.searchControl.value || undefined,
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

  openDialog(employee?: EmployeeDto): void {
    const dialogRef = this.dialog.open(EmployeeDialogComponent, {
      width: '500px',
      data: employee
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.refreshData();
      }
    });
  }

  deleteEmployee(employee: EmployeeDto): void {
    const dialogRef = this.dialog.open(ConfirmDialogComponent, {
      data: {
        title: 'Delete Employee',
        message: `Are you sure you want to delete ${employee.fullName}?`
      }
    });

    dialogRef.afterClosed().subscribe(confirm => {
      if (confirm) {
        this.employeeService.delete(employee.id).subscribe(() => {
          this.snackBar.open('Employee deleted successfully', 'Close', { duration: 3000 });
          this.refreshData();
        });
      }
    });
  }

  private refreshData(): void {
    // Trigger the merge observable by manually causing a paginator event
    this.paginator.page.emit();
  }
}
