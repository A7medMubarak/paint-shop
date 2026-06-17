import { useState, useCallback, type ReactNode } from 'react';
import { AuthContext } from './AuthContextValue';
import { authService } from '../services/auth';
import type { LoginRequest } from '../types';

export function AuthProvider({ children }: { children: ReactNode }) {
  const [token, setToken] = useState<string | null>(localStorage.getItem('token'));
  const [role, setRole] = useState<string | null>(localStorage.getItem('role'));
  const [username, setUsername] = useState<string | null>(localStorage.getItem('username'));

  const login = useCallback(async (data: LoginRequest) => {
    const response = await authService.login(data);
    authService.storeSession(response);
    setToken(response.token);
    setRole(response.role);
    setUsername(response.username);
    return response;
  }, []);

  const logout = useCallback(() => {
    authService.logout();
    setToken(null);
    setRole(null);
    setUsername(null);
  }, []);

  return (
    <AuthContext.Provider value={{
      token, role, username,
      isOwner: role === 'Owner',
      isAuthenticated: !!token,
      login, logout
    }}>
      {children}
    </AuthContext.Provider>
  );
}
