import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { TaskDto, CreateTaskRequest, UpdateTaskRequest, TaskFilterQuery } from './tasks.model';
import { PagedResult } from '../employees/employees.model';

@Injectable({
  providedIn: 'root'
})
export class TaskService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = `${environment.apiUrl}/tasks`;

  getAll(query: TaskFilterQuery): Observable<PagedResult<TaskDto>> {
    let params = new HttpParams()
      .set('page', query.page.toString())
      .set('pageSize', query.pageSize.toString());

    if (query.search) params = params.set('search', query.search);
    if (query.status) params = params.set('status', query.status.toString());
    if (query.priority) params = params.set('priority', query.priority.toString());
    if (query.assignedToId) params = params.set('assignedToId', query.assignedToId);
    if (query.sortBy) params = params.set('sortBy', query.sortBy);
    if (query.sortDirection) params = params.set('sortDirection', query.sortDirection);

    return this.http.get<PagedResult<TaskDto>>(this.apiUrl, { params });
  }

  getById(id: string): Observable<TaskDto> {
    return this.http.get<TaskDto>(`${this.apiUrl}/${id}`);
  }

  create(request: any): Observable<TaskDto> {
    // Manually construct the payload to ensure correct property names and types
    const payload = {
      title: request.title,
      description: request.description || '',
      priority: Number(request.priority),
      status: Number(request.status),
      startDate: new Date(request.startDate).toISOString(),
      dueDate: new Date(request.dueDate).toISOString(),
      assignedToId: request.assignedToId
    };
    return this.http.post<TaskDto>(this.apiUrl, payload);
  }

  update(id: string, request: any): Observable<TaskDto> {
    const payload = {
      title: request.title,
      description: request.description || '',
      priority: Number(request.priority),
      status: Number(request.status),
      startDate: new Date(request.startDate).toISOString(),
      dueDate: new Date(request.dueDate).toISOString(),
      assignedToId: request.assignedToId
    };
    return this.http.put<TaskDto>(`${this.apiUrl}/${id}`, payload);
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }

  uploadAttachment(id: string, file: File): Observable<TaskDto> {
    const formData = new FormData();
    formData.append('file', file);
    return this.http.post<TaskDto>(`${this.apiUrl}/${id}/attachment`, formData);
  }
}
