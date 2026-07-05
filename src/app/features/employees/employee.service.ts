import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { EmployeeDto, CreateEmployeeRequest, UpdateEmployeeRequest, PagedResult, PaginationQuery } from './employees.model';

@Injectable({
  providedIn: 'root'
})
export class EmployeeService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = `${environment.apiUrl}/employees`;

  getLookup(): Observable<EmployeeDto[]> {
    return this.http.get<EmployeeDto[]>(`${this.apiUrl}/lookup`);
  }

  getAll(query: PaginationQuery): Observable<PagedResult<EmployeeDto>> {
    let params = new HttpParams()
      .set('page', query.page.toString())
      .set('pageSize', query.pageSize.toString());

    if (query.search) params = params.set('search', query.search);
    if (query.sortBy) params = params.set('sortBy', query.sortBy);
    if (query.sortDirection) params = params.set('sortDirection', query.sortDirection);

    return this.http.get<PagedResult<EmployeeDto>>(this.apiUrl, { params });
  }

  getById(id: string): Observable<EmployeeDto> {
    return this.http.get<EmployeeDto>(`${this.apiUrl}/${id}`);
  }

  create(employee: CreateEmployeeRequest): Observable<EmployeeDto> {
    return this.http.post<EmployeeDto>(this.apiUrl, employee);
  }

  update(id: string, employee: UpdateEmployeeRequest): Observable<EmployeeDto> {
    return this.http.put<EmployeeDto>(`${this.apiUrl}/${id}`, employee);
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}
