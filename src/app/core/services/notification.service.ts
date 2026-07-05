import { Injectable, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap, map } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface NotificationDto {
  id: string;
  message: string;
  type: number;
  isRead: boolean;
  createdAt: string;
  taskId?: string;
}

@Injectable({
  providedIn: 'root'
})
export class NotificationService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = `${environment.apiUrl}/notifications`;
  
  unreadCount = signal(0);

  getAll(): Observable<NotificationDto[]> {
    return this.http.get<NotificationDto[]>(this.apiUrl).pipe(
      map(list => list || []), // Ensure it's always an array
      tap(list => {
        const count = list.filter(n => !n.isRead).length;
        this.unreadCount.set(count);
      })
    );
  }

  markAsRead(id: string): Observable<void> {
    return this.http.patch<void>(`${this.apiUrl}/${id}/read`, {});
  }

  markAllAsRead(): Observable<void> {
    return this.http.patch<void>(`${this.apiUrl}/read-all`, {});
  }
}
