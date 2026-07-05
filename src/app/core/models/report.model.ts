export interface AdminDashboardStats {
  totalEmployees: number;
  totalTasks: number;
  completedTasks: number;
  pendingTasks: number;
  overdueTasks: number;
}

export interface EmployeeDashboardStats {
  myTasks: number;
  completedTasks: number;
  pendingTasks: number;
  overdueTasks: number;
}

export interface TaskReportDto {
  id: string;
  title: string;
  assignedTo: string;
  priority: string;
  status: string;
  dueDate: string;
}

export interface EmployeeReportDto {
  employeeName: string;
  department?: string;
  totalTasks: number;
  completedTasks: number;
  pendingTasks: number;
}
