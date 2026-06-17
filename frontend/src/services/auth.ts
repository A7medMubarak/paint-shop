import { api } from './api';
import type { LoginRequest, LoginResponse } from '../types';

export const authService = {
  login: (data: LoginRequest) => api.post<LoginResponse>('/auth/login', data),
  logout: () => {
    localStorage.removeItem('token');
    localStorage.removeItem('role');
    localStorage.removeItem('username');
  },
  isAuthenticated: () => !!localStorage.getItem('token'),
  isOwner: () => localStorage.getItem('role') === 'Owner',
  storeSession: (response: LoginResponse) => {
    localStorage.setItem('token', response.token);
    localStorage.setItem('role', response.role);
    localStorage.setItem('username', response.username);
  }
};
