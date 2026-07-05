import { User } from '../../core/models/auth.model';

export interface Employee extends User {
  isActive: boolean;
  createdAt: string;
  updatedAt?: string;
}

export interface EmployeeDto extends Employee {}

export interface CreateEmployeeRequest {
  fullName: string;
  email: string;
  password?: string;
  department?: string;
  designation?: string;
}

export interface UpdateEmployeeRequest {
  fullName: string;
  email: string;
  department?: string;
  designation?: string;
  isActive: boolean;
}

export interface PaginationQuery {
  page: number;
  pageSize: number;
  search?: string;
  sortBy?: string;
  sortDirection?: 'asc' | 'desc';
}

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
}
