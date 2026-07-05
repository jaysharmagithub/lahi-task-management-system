export enum UserRole {
  Admin = 1,
  Employee = 2
}

export interface User {
  id: string;
  fullName: string;
  email: string;
  role: UserRole;
  department?: string;
  designation?: string;
}

export interface AuthResponse {
  accessToken: string;
  refreshToken: string;
  accessTokenExpiry: string;
  user: User;
}

export interface LoginRequest {
  email: string;
  password: string;
  rememberMe: boolean;
}

export interface RegisterRequest {
  fullName: string;
  email: string;
  password: string;
  confirmPassword: string;
  role: UserRole;
  department?: string;
  designation?: string;
}
