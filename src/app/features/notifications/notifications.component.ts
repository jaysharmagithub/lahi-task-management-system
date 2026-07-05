import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatListModule } from '@angular/material/list';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatDividerModule } from '@angular/material/divider';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatTooltipModule } from '@angular/material/tooltip';
import { NotificationService, NotificationDto } from '../../core/services/notification.service';

@Component({
  selector: 'app-notifications',
  standalone: true,
  imports: [
    CommonModule,
    MatListModule,
    MatIconModule,
    MatButtonModule,
    MatCardModule,
    MatDividerModule,
    MatProgressBarModule,
    MatTooltipModule
  ],
  template: `
    <div class="page-container">
      <header class="page-header">
        <h1>Notifications</h1>
        <button mat-stroked-button (click)="markAllRead()" [disabled]="notifications().length === 0">
          Mark All as Read
        </button>
      </header>

      <mat-card class="notification-card">
        <mat-progress-bar *ngIf="isLoading()" mode="indeterminate"></mat-progress-bar>
        
        <mat-list *ngIf="notifications().length > 0; else emptyState">
          <ng-container *ngFor="let note of notifications(); let last = last">
            <mat-list-item [class.unread]="!note.isRead">
              <mat-icon matListItemIcon [color]="getIconColor(note.type)">{{getIcon(note.type)}}</mat-icon>
              <div matListItemTitle [style.font-weight]="note.isRead ? 'normal' : 'bold'">{{note.message}}</div>
              <div matListItemLine>{{note.createdAt | date:'medium'}}</div>
              <div matListItemMeta>
                <button mat-icon-button *ngIf="!note.isRead" (click)="markRead(note.id)" matTooltip="Mark as read">
                  <mat-icon>check_circle_outline</mat-icon>
                </button>
              </div>
            </mat-list-item>
            <mat-divider *ngIf="!last"></mat-divider>
          </ng-container>
        </mat-list>

        <ng-template #emptyState>
          <div class="empty-state">
            <mat-icon>notifications_none</mat-icon>
            <p>No notifications yet.</p>
          </div>
        </ng-template>
      </mat-card>
    </div>
  `,
  styles: [`
    .page-container { padding: 8px; }
    .page-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 24px; }
    .unread { background-color: rgba(63, 81, 181, 0.05); }
    .empty-state { padding: 48px; text-align: center; color: rgba(0,0,0,0.54); }
    .empty-state mat-icon { font-size: 48px; width: 48px; height: 48px; margin-bottom: 16px; }
  `]
})
export class NotificationsComponent implements OnInit {
  private service = inject(NotificationService);
  
  notifications = signal<NotificationDto[]>([]);
  isLoading = signal(false);

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.isLoading.set(true);
    this.service.getAll().subscribe({
      next: (res) => { this.notifications.set(res); this.isLoading.set(false); },
      error: () => this.isLoading.set(false)
    });
  }

  markRead(id: string): void {
    this.service.markAsRead(id).subscribe(() => this.load());
  }

  markAllRead(): void {
    this.service.markAllAsRead().subscribe(() => this.load());
  }

  getIcon(type: number): string {
    // 1: TaskAssigned, 2: TaskDueSoon, 3: TaskCompleted
    if (type === 1) return 'assignment_ind';
    if (type === 2) return 'notification_important';
    if (type === 3) return 'task_alt';
    return 'info';
  }

  getIconColor(type: number): string {
    if (type === 2) return 'warn';
    if (type === 3) return 'primary';
    return '';
  }
}
