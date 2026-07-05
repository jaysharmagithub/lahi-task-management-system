import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { AdminDashboardStats, EmployeeDashboardStats, TaskReportDto, EmployeeReportDto } from '../models/report.model';

@Injectable({
  providedIn: 'root'
})
export class ReportService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = `${environment.apiUrl}/reports`;

  getAdminDashboard(): Observable<AdminDashboardStats> {
    return this.http.get<AdminDashboardStats>(`${this.apiUrl}/dashboard/admin`);
  }

  getEmployeeDashboard(): Observable<EmployeeDashboardStats> {
    return this.http.get<EmployeeDashboardStats>(`${this.apiUrl}/dashboard/employee`);
  }

  getCompletedTasks(): Observable<TaskReportDto[]> {
    return this.http.get<TaskReportDto[]>(`${this.apiUrl}/completed`);
  }

  getPendingTasks(): Observable<TaskReportDto[]> {
    return this.http.get<TaskReportDto[]>(`${this.apiUrl}/pending`);
  }

  getEmployeeWiseReport(): Observable<EmployeeReportDto[]> {
    return this.http.get<EmployeeReportDto[]>(`${this.apiUrl}/employee-wise`);
  }

  exportToExcel(reportType: 'completed' | 'pending' | 'employee'): Observable<Blob> {
    return this.http.get(`${this.apiUrl}/export/excel/${reportType}`, { responseType: 'blob' });
  }

  exportToCsv(reportType: 'completed' | 'pending' | 'employee'): Observable<Blob> {
    return this.http.get(`${this.apiUrl}/export/csv/${reportType}`, { responseType: 'blob' });
  }
}
