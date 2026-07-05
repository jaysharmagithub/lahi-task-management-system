export enum TaskPriority {
  Low = 1,
  Medium = 2,
  High = 3,
  Critical = 4
}

export enum TaskStatus {
  Pending = 1,
  InProgress = 2,
  Completed = 3,
  Overdue = 4
}

export interface TaskDto {
  id: string;
  title: string;
  description?: string;
  priority: TaskPriority;
  status: TaskStatus;
  startDate: string;
  dueDate: string;
  attachmentFileName?: string;
  assignedToId: string;
  assignedToName: string;
  createdById: string;
  createdByName: string;
  createdAt: string;
  updatedAt?: string;
}

export interface CreateTaskRequest {
  title: string;
  description?: string;
  priority: TaskPriority;
  status: TaskStatus;
  startDate: string;
  dueDate: string;
  assignedToId: string;
}

export interface UpdateTaskRequest extends CreateTaskRequest {}

export interface TaskFilterQuery {
  page: number;
  pageSize: number;
  search?: string;
  sortBy?: string;
  sortDirection?: 'asc' | 'desc';
  status?: TaskStatus;
  priority?: TaskPriority;
  assignedToId?: string;
}
